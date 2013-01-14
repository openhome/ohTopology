# Defines the build behaviour for continuous integration builds.
#
# Invoke with "go hudson_build"

# Maintenance notes:
#
# The following special functions are available for use in this file:
#
# add_option("-t", "--target", help="Set the target.")
#     Add a command-line option. See Python's optparse for arguments.
#     options are accessed on context.options. (See build_step.)
#
# fetch_dependencies("ohnet", "nunit", "zwave", platform="Linux-ARM")
# fetch_dependencies(["ohnet", "log4net"], platform="Windows-x86")
#     Fetches the specified dependencies for the specified platform. Omit platform
#     to use the platform defined in the OH_PLATFORM environment variable.
#     Omit dependency names to fetch everything.
#
# get_dependency_args("ohnet", "nunit", "zwave")
# get_dependency_args(["ohnet", "log4net"])
#     Returns a list of the arguments found in the dependencies.txt file
#     for the given dependencies, using the current environment.
#
# @build_step("name", optional=True, default=False)
# @build_condition(OH_PLATFORM="Linux-x86")
# @build_condition(OH_PLATFORM="Windoxs-x86")
# def your_build_step(context):
#     ...
#     Add a new build-step that only runs when one of the build conditions
#     matches. (Here if OH_PLATFORM is either "Linux-x86" or "Windows-x86".)
#     Context will be an object with context.options and context.env defined.
#     Name argument is optional and defaults to the name of the function. If
#     optional is set to True you can enable or disable the step with
#     select_optional_steps, and default determines whether it will run by
#     default.
#
# select_optional_steps("+build", "-test")
# select_optional_steps("stresstest", disable_others=True)
#     Enables or disables optional steps. Use "+foo" to enable foo, and "-foo"
#     to disable it. Use 'disable_others=True' to disable all optional steps
#     other than those specifically enabled.
#
# python("waf", "build")
#     Invoke a Python subprocess. Provide arguments as strings or lists of
#     strings.
#
# rsync(...)
#     Invoke an rsync subprocess. See later for examples.
#
# shell(...)
#     Invoke a shell subprocess. Arguments similar to python().
#
# with SshSession(host, username) as ssh:
#     ssh("echo", "hello")
#
#     Connect via ssh and issue commands. Command arguments similar to python().
#  

from ci import (
        build_step, require_version, add_option, specify_optional_steps,
        build_condition, default_platform, get_dependency_args,
        get_vsvars_environment, fetch_dependencies, python, scp, shell, cli)

require_version(22)

solutions = [
    {
     "sln":"src/ohTopology.sln",
     "mdtool":False
    }
]


# Command-line options. See documentation for Python's optparse module.
add_option("-t", "--target", help="Target platform. One of Windows-x86, Windows-x64, Linux-x86, Linux-x64, Linux-ARM.")
add_option("-a", "--artifacts", help="Build artifacts directory. Used to fetch dependencies.")
add_option("--debug", action="store_const", const="debug", dest="debugmode", default="Debug", help="Build debug version.")
add_option("--release", action="store_const", const="release", dest="debugmode", help="Build release version.")
add_option("--steps", default="default", help="Steps to run, comma separated. (all,default,fetch,configure,build,tests,publish)")
add_option("--publish-version", action="store", help="Specify version string.")
add_option("--fetch-only", action="store_const", const="fetch", dest="steps", help="Fetch dependencies only.")

@build_step()
def choose_optional_steps(context):
    specify_optional_steps(context.options.steps)

# Unconditional build step. Choose a platform and set the
# appropriate environment variable.
@build_step()
def choose_platform(context):
    if context.options.target:
        context.env["OH_PLATFORM"] = context.options.target
    elif "PLATFORM" in context.env:
        context.env["OH_PLATFORM"] = {
                "Windows-x86" : "Windows-x86",
                "Windows-x64" : "Windows-x64",
                "Linux-x86" : "Linux-x86",
                "Linux-x64" : "Linux-x64",
                "Linux-ARM" : "Linux-ARM",
                "Mac-x86" : "Mac-x86",
                "Mac-x64" : "Mac-x64",
            }[context.env["PLATFORM"]]
    else:
        context.env["OH_PLATFORM"] = default_platform()
    context.env.update(MSBUILDCONFIGURATION="Release" if context.options.debugmode=="release" else "Debug")

# Universal build configuration.
@build_step()
def setup_universal(context):
    env = context.env
    env.update(
        OHNET_ARTIFACTS=context.options.artifacts or 'http://www.openhome.org/releases/artifacts',
        OH_PUBLISHDIR="releases@www.openhome.org:/home/releases/www/artifacts",
        OH_PROJECT="ohTopology",
        OH_DEBUG=context.options.debugmode,
        BUILDDIR='buildhudson',
        WAFLOCK='.lock-wafbuildhudson',
        OH_VERSION=context.options.publish_version or context.env.get('RELEASE_VERSION', 'UNKNOWN'))
    context.configure_args = get_dependency_args(env={'debugmode':context.env['OH_DEBUG']})
    context.configure_args += ["--dest-platform", env["OH_PLATFORM"]]
    context.configure_args += ["--" + context.options.debugmode.lower()]

# Extra Windows build configuration.
@build_step()
@build_condition(OH_PLATFORM="Windows-x86")
@build_condition(OH_PLATFORM="Windows-x64")
def setup_windows(context):
    env = context.env
    env.update(
        OPENHOME_NO_ERROR_DIALOGS="1",
        OHNET_NO_ERROR_DIALOGS="1")
    env.update(get_vsvars_environment())
    context.env.update(MSBUILDCMD="msbuild /nologo /p:Configuration=%s" % (context.env["MSBUILDCONFIGURATION"]))
    context.env.update(MSBUILDTARGETSWITCH="/t:")
    context.env.update(MSBUILDSOLUTIONSUFFIX="Windows")
    
@build_condition(OH_PLATFORM="Windows-x86")
def setup_windows_x86(context):
    context.env.update(get_vsvars_environment("x86"))
    
@build_condition(OH_PLATFORM="Windows-x64")
def setup_windows_x64(context):
    context.env.update(get_vsvars_environment("amd64"))

# Extra Linux build configuration.
@build_step()
@build_condition(OH_PLATFORM="Linux-x86")
@build_condition(OH_PLATFORM="Linux-x64")
@build_condition(OH_PLATFORM="Linux-ARM")
def setup_linux(context):
    env = context.env
    context.env.update(MDTOOLBUILDCMD="mdtool build -c:%s|Any CPU" % context.env["MSBUILDCONFIGURATION"])
    context.env.update(MSBUILDCMD="xbuild /nologo /p:Configuration=%s" % (context.env["MSBUILDCONFIGURATION"]))
    
# Extra Mac build configuration.
@build_step()
@build_condition(OH_PLATFORM="Mac-x86")
@build_condition(OH_PLATFORM="Mac-x64")
@build_condition(OH_PLATFORM="Mac-ARM")
def setup_mac(context):
    context.env.update(MDTOOLBUILDCMD="/Applications/MonoDevelop.app/Contents/MacOS/mdtool build -c:%s|Any CPU" % context.env["MSBUILDCONFIGURATION"])
    context.env.update(MSBUILDCMD="xbuild /nologo /p:Configuration=%s" % (context.env["MSBUILDCONFIGURATION"]))

# Principal build steps.
@build_step("fetch", optional=True)
def fetch(context):
    fetch_dependencies(env={'debugmode':context.env['OH_DEBUG'],
                     'titlecase-debugmode':context.options.debugmode.title()})

@build_step("configure", optional=True)
def configure(context):
    python("waf", "configure", context.configure_args)

@build_step("clean", optional=True)
def clean(context):
    python("waf", "clean")
    for solution in solutions:
        do_build(context, solution, "Clean")

@build_step("build", optional=True)
def build(context):
    python("waf")
    for solution in solutions:
        do_build(context, solution, "Build")

@build_step("test", optional=True)
def test(context):
    python("waf", "test")
    cli(['build/ohTopology/AnyPlatform/' + context.options.debugmode.title() + '/bin/TestTopology1.exe', 'build/ohTopology/AnyPlatform/' + context.options.debugmode.title()+ '/bin/Topology1TestScript.txt'])
    cli(['build/ohTopology/AnyPlatform/' + context.options.debugmode.title() + '/bin/TestTopology2.exe', 'build/ohTopology/AnyPlatform/' + context.options.debugmode.title() + '/bin/Topology2TestScript.txt'])
    cli(['build/ohTopology/AnyPlatform/' + context.options.debugmode.title() + '/bin/TestTopology3.exe', 'build/ohTopology/AnyPlatform/' + context.options.debugmode.title() + '/bin/Topology3TestScript.txt'])
    cli(['build/ohTopology/AnyPlatform/' + context.options.debugmode.title() + '/bin/TestTopology4.exe', 'build/ohTopology/AnyPlatform/' + context.options.debugmode.title() + '/bin/Topology4TestScript.txt'])

@build_step("publish", optional=True, default=False)
def publish(context):
    targetpath    = "{OH_PUBLISHDIR}/{OH_PROJECT}/{OH_PROJECT}-{OH_VERSION}-{OH_PLATFORM}-{OH_DEBUG}.tar.gz".format(**context.env)
    sourcepath    = "{BUILDDIR}/{OH_PROJECT}.tar.gz".format(**context.env)
    scp(sourcepath,    targetpath)
    
def do_build(context, solution, target):
    mdtool = solution["mdtool"]
    solutionfile = solution["sln"]
    msbuild = context.env['MDTOOLBUILDCMD'] if mdtool else context.env['MSBUILDCMD']
    targetswitch =  "-t:" if mdtool else "/t:"
    buildshell = "%(msbuild)s %(sln)s %(targetswitch)sBuild" % {'sln':solutionfile, 'msbuild':msbuild, 'targetswitch':targetswitch}
    context.env["PLATFORM"] = ""
    shell(buildshell)
