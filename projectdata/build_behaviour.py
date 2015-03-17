from ci import (OpenHomeBuilder, require_version)

require_version(41)

class Builder(OpenHomeBuilder):
    native_package_location = 'buildhudson/{packagename}'

    def setup(self):
        self.env.update(
            WAFLOCK='.lock-wafbuildhudson',
            )
        self.configure_args = self.get_dependency_args(env={'debugmode':self.configuration})
        self.configure_args += ["--dest-platform", self.platform]
        self.configure_args += ["--" + self.configuration.lower()]

    def configure(self):
        self.python("waf", "configure", *self.configure_args)

    def clean(self):
        self.python('waf', 'clean')

    def build(self):
        self.python('waf')

    def test(self):
        self.python("waf", "test")

    def publish(self):
        # Publish native packages regardless of platform
        self.publish_package(
                'ohTopology.tar.gz',
                'ohTopology/ohTopology-{version}-{platform}-{configuration}.tar.gz',
                package_location = self.native_package_location)
