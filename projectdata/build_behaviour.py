from ci import (OpenHomeBuilder, require_version)

require_version(34)

solutions = [
    {
     "sln":"src/ohTopology.sln",
     "mdtool":False
    }
]

class Builder(OpenHomeBuilder):
    test_location = 'build/{assembly}/bin/{configuration}/{assembly}.dll'

    # Standard rules enforce warnings-as-errors and importing SharedSettings.targets,
    # disallow tabs in C# files and disallow .orig files in the source tree.
    source_check_rules = OpenHomeBuilder.standard_source_check_rules + [
            ['src/ohTopology/Cp*.cs', '-no-tabs'],   # Suppress the no-tabs rule for generated files.
        ]

    managed_package_location = 'build/packages/{packagename}'
    native_package_location = 'buildhudson/{packagename}'

    def setup(self):
        self.env.update(
            WAFLOCK='.lock-wafbuildhudson',
            )
        self.configure_args = self.get_dependency_args(env={'debugmode':self.configuration})
        self.configure_args += ["--dest-platform", self.platform]
        self.configure_args += ["--" + self.configuration.lower()]
        self.set_nunit_location('dependencies/nuget/NUnit.Runners.2.6.1/tools/nunit-console-x86.exe')

    def configure(self):
        self.python("waf", "configure", self.configure_args)

    def clean(self):
        self.python('waf', 'clean')
        for solution in solutions:
            self.do_build(solution, 'Clean')

    def build(self):
        self.python('waf')
        for solution in solutions:
            self.do_build(solution, 'Build')

    def do_build(self, solution, target):
        mdtool = solution["mdtool"]
        solutionfile = solution["sln"]
        self.env["PLATFORM"] = "" # Not sure why - presumably this env var messes up something?
        if mdtool:
            self.mdtool(solutionfile, target=target)
        else:
            self.msbuild(solutionfile, target=target)

    def test(self):
        self.python("waf", "test")

        substitutions = { 'debugmode' : self.configuration.title() }
        self.cli(['build/TestIdCache/bin/%(debugmode)s/TestIdCache.exe' % substitutions])
        self.cli(['build/TestMediaEndpoint/bin/%(debugmode)s/TestMediaEndpoint.exe' % substitutions])
        self.cli(['build/TestMediaServer/bin/%(debugmode)s/TestMediaServer.exe' % substitutions])
        def run_test(test):
            prog = 'build/Test%(test)s/bin/%(debugmode)s/Test%(test)s.exe'
            scrp = 'build/Test%(test)s/bin/%(debugmode)s/%(test)sTestScript.txt'
            fmt = { 'test' : test, 'debugmode' : self.configuration.title() }
            self.cli([prog % fmt, scrp % fmt])
        run_test('Topology1')
        run_test('Topology2')
        run_test('Topologym')
        run_test('Topology3')
        run_test('Topology4')
        run_test('StandardHouse')
        run_test('Zone')
        run_test('Registration')


    def publish(self):
        # Publish native packages regardless of platform
        self.publish_package(
                'ohTopology.tar.gz',
                'ohTopology/ohTopology-{version}-{platform}-{configuration}.tar.gz',
                package_location = self.native_package_location)

        if self.options.auto and not self.platform == 'Windows-x86':
            # Only publish .net binaries from one CI platform, Windows-x86.
            return

        self.publish_package(
                'ohTopology.net-AnyPlatform-{configuration}.tar.gz',
                'ohTopology/ohTopology.net-{version}-AnyPlatform-{configuration}.tar.gz',
                package_location = self.managed_package_location)

