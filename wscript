#!/usr/bin/python2
import sys
import os

from waflib.Node import Node
from wafmodules.filetasks import (
    find_resource_or_fail)

import os.path, sys
sys.path[0:0] = [os.path.join('dependencies', 'AnyPlatform', 'ohWafHelpers')]

from filetasks import gather_files, build_tree, copy_task
from utilfuncs import invoke_test, guess_dest_platform, configure_toolchain, guess_ohnet_location

def options(opt):
    opt.load('msvc')
    opt.load('compiler_cxx')
    opt.load('compiler_c')
    opt.add_option('--ohnet-include-dir', action='store', default=None)
    opt.add_option('--ohnet-lib-dir', action='store', default=None)
    opt.add_option('--ohnet', action='store', default=None)
    opt.add_option('--debug', action='store_const', dest="debugmode", const="Debug", default="Release")
    opt.add_option('--release', action='store_const', dest="debugmode",  const="Release", default="Release")
    opt.add_option('--dest-platform', action='store', default=None)
    opt.add_option('--cross', action='store', default=None)
    #opt.add_option('--big-endian', action='store_const', dest="endian",  const="BIG", default="LITTLE")
    #opt.add_option('--little-endian', action='store_const', dest="endian",  const="LITTLE", default="LITTLE")
    #opt.add_option('--dest', action='store', default=None)

def configure(conf):

    def set_env(conf, varname, value):
        conf.msg(
                'Setting %s to' % varname,
                "True" if value is True else
                "False" if value is False else
                value)
        setattr(conf.env, varname, value)
        return value

    conf.msg("debugmode:", conf.options.debugmode)
    if conf.options.dest_platform is None:
        try:
            conf.options.dest_platform = guess_dest_platform()
        except KeyError:
            conf.fatal('Specify --dest-platform')

    configure_toolchain(conf)
    guess_ohnet_location(conf)

    if conf.options.dest_platform in ['Windows-x86', 'Windows-x64']:
        conf.env.LIB_OHNET=['ws2_32', 'iphlpapi', 'dbghelp']
    conf.env.STLIB_OHNET=['ohNetProxies', 'TestFramework', 'ohNetCore']
    conf.env.INCLUDES = [
        '.',
        conf.path.find_node('.').abspath()
        ]

    mono = set_env(conf, 'MONO', [] if conf.options.dest_platform.startswith('Windows') else ["mono", "--runtime=v4.0"])


def get_node(bld, node_or_filename):
    if isinstance(node_or_filename, Node):
        return node_or_filename
    return bld.path.find_node(node_or_filename)

def create_copy_task(build_context, files, target_dir='', cwd=None, keep_relative_paths=False, name=None):
    source_file_nodes = [get_node(build_context, f) for f in files]
    if keep_relative_paths:
        cwd_node = build_context.path.find_dir(cwd)
        target_filenames = [
                path.join(target_dir, source_node.path_from(cwd_node))
                for source_node in source_file_nodes]
    else:
        target_filenames = [
                os.path.join(target_dir, source_node.name)
                for source_node in source_file_nodes]
        target_filenames = map(build_context.bldnode.make_node, target_filenames)
    return build_context(
            rule=copy_task,
            source=source_file_nodes,
            target=target_filenames,
            name=name)

class GeneratedFile(object):
    def __init__(self, xml, domain, type, version, target):
        self.xml = xml
        self.domain = domain
        self.type = type
        self.version = version
        self.target = target

upnp_services = [
        GeneratedFile('OpenHome/Av/ServiceXml/OpenHome/Product1.xml', 'av.openhome.org', 'Product', '1', 'AvOpenhomeOrgProduct1'),
        GeneratedFile('OpenHome/Av/ServiceXml/OpenHome/Volume1.xml', 'av.openhome.org', 'Volume', '1', 'AvOpenhomeOrgVolume1'),
    ]

def build(bld):

    # Generated provider base classes
    t4templatedir = bld.env['T4_TEMPLATE_PATH']
    text_transform_exe_node = find_resource_or_fail(bld, bld.root, os.path.join(bld.env['TEXT_TRANSFORM_PATH'], 'TextTransform.exe'))
    for service in upnp_services:
        for t4Template, prefix, ext, args in [
                ('CpUpnpCppHeader.tt', 'Cp', '.h', '-a buffer:1'),
                ('CpUpnpCppBufferSource.tt', 'Cp', '.cpp', '')
                ]:
            t4_template_node = find_resource_or_fail(bld, bld.root, os.path.join(t4templatedir, t4Template))
            tgt = bld.path.find_or_declare(os.path.join('Generated', prefix + service.target + ext))
            bld(
                rule="${MONO} " + text_transform_exe_node.abspath() + " -o " + tgt.abspath() + " " + t4_template_node.abspath() + " -a xml:../" + service.xml + " -a domain:" + service.domain + " -a type:" + service.type + " -a version:" + service.version + " " + args,
                source=[text_transform_exe_node, t4_template_node, service.xml],
                target=tgt
                )
    bld.add_group()

    create_copy_task(bld, ['OpenHome/Av/CpTopology.h'], 'Include/OpenHome/Av')
    bld.add_group()

    # Library
    bld.stlib(
            source=[
                'OpenHome/Av/CpTopology.cpp',
                'OpenHome/Av/CpTopology1.cpp',
                'OpenHome/Av/CpTopology2.cpp',
                'OpenHome/Av/CpTopology3.cpp',
                'OpenHome/Av/CpTopology4.cpp',
                'Generated/CpAvOpenhomeOrgProduct1.cpp',
                'Generated/CpAvOpenhomeOrgVolume1.cpp'
            ],
            use=['OHNET'],
            target='ohTopology')

    # Tests
    bld.program(
            source='OpenHome/Av/Tests/TestTopology1.cpp',
            use=['OHNET', 'ohTopology'],
            target='TestTopology1')
    bld.program(
            source='OpenHome/Av/Tests/TestTopology2.cpp',
            use=['OHNET', 'ohTopology'],
            target='TestTopology2')
    bld.program(
            source='OpenHome/Av/Tests/TestTopology3.cpp',
            use=['OHNET', 'ohTopology'],
            target='TestTopology3')
    bld.program(
            source='OpenHome/Av/Tests/TestTopology4.cpp',
            use=['OHNET', 'ohTopology'],
            target='TestTopology4')
    bld.program(
            source='OpenHome/Av/Tests/TestTopology.cpp',
            use=['OHNET', 'ohTopology'],
            target='TestTopology')

    # Bundles
    header_files = gather_files(bld, '{top}/OpenHome/Av', ['*.h'])
    lib_files = gather_files(bld, '{bld}', [bld.env.cxxstlib_PATTERN % 'ohTopology'])
    bundle_files = build_tree({
        'ohTopology/lib' : lib_files,
        'ohTopology/Include/OpenHome/Av' : header_files
        })
    bundle_files.create_tgz_task(bld, 'ohTopology.tar.gz')

# == Command for invoking unit tests ==

def test(tst):
    for t, a, when in [['TestTopology', [], True]
                      ,['TestTopology1', ['--mx', '3'], True]
                      ,['TestTopology2', ['--duration', '10'], True]
                      ,['TestTopology3', ['--duration', '10'], True]
                      ,['TestTopology4', ['--duration', '10'], True]
                      ]:
        tst(rule=invoke_test, test=t, args=a, always=when)
        tst.add_group() # Don't start another test until first has finished.


# == Contexts to make 'waf test' work ==

from waflib.Build import BuildContext

class TestContext(BuildContext):
    cmd = 'test'
    fun = 'test'

# vim: set filetype=python softtabstop=4 expandtab shiftwidth=4 tabstop=4:
