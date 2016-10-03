# Disable the stupid stuff rpm distros include in the build process by default:
#   Disable any prep shell actions. replace them with simply 'true'
%define __spec_prep_post true
%define __spec_prep_pre true
#   Disable any build shell actions. replace them with simply 'true'
%define __spec_build_post true
%define __spec_build_pre true
#   Disable any install shell actions. replace them with simply 'true'
%define __spec_install_post true
%define __spec_install_pre true
#   Disable any clean shell actions. replace them with simply 'true'
%define __spec_clean_post true
%define __spec_clean_pre true
# Disable checking for unpackaged files ?
#%undefine __check_files

# Use md5 file digest method.
# The first macro is the one used in RPM v4.9.1.1
%define _binary_filedigest_algorithm 1
# This is the macro I find on OSX when Homebrew provides rpmbuild (rpm v5.4.14)
%define _build_binary_file_digest_algo 1

# Use bzip2 payload compression
%define _binary_payload w9.bzdio

#
# The Preamble
#
Name: palette-insight-agent
Version: %{version}
Release: %{buildrelease}
Summary: Palette Insight Agent
Group: default
License: commercial
Vendor: palette-software.net
URL: http://www.palette-software.com
Packager: Palette Developers <developers@palette-software.com>
BuildArch: noarch
# Disable Automatic Dependency Processing
AutoReqProv: no
# Add prefix, must not end with / except for root (/)
Prefix: /
# Seems specifying BuildRoot is required on older rpmbuild (like on CentOS 5)
# fpm passes '--define buildroot ...' on the commandline, so just reuse that.
# BuildRoot: %buildroot

Requires(pre): palette-insight-server >= 400:2.0.0

%description
Palette Insight GP Agent

# Install directory - without / prefix
%define install_dir opt/insight-agent/

%pre
# noop

%postun
# noop

%prep
mkdir -p %{install_dir}
pushd %{install_dir}
curl -O https://github.com/palette-software/PaletteInsightAgent/releases/download/v%{version}/Palette-Insight-v%{version}-installer.msi
popd

%build
# noop

%install
# noop

%post
# noop

%clean
pushd %{install_dir}
rm *
popd
rmdir -p %{install_dir}

%files
%defattr(-,insight,insight,-)

# Reject config files already listed or parent directories, then prefix files
# with "/", then make sure paths with spaces are quoted.
%dir /%{install_dir}
/%{install_dir}/Palette-Insight-v%{version}-installer.msi

%changelog
