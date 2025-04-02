VERSION="0.1.0"
RUNTIME_OUTPUT_DIR=out/shared/Laniakea.NET/$(VERSION)/
SDK_OUTPUT_DIR=out/sdk/Sdks/Laniakea.NET.Sdk/
# DOTNET_SDK_VERSION=$(shell dotnet --list-sdks | head -1 | cut -d ' ' -f 1)

default:
	dotnet build --configuration Release Laniakea.NET.sln

runtime:
	mkdir -p $(RUNTIME_OUTPUT_DIR)
	cp build/Release/Laniakea.NET.Core.dll $(RUNTIME_OUTPUT_DIR)/
	cp runtime/Laniakea.NET.deps.json $(RUNTIME_OUTPUT_DIR)/
	cp runtime/Laniakea.NET.runtimeconfig.json $(RUNTIME_OUTPUT_DIR)/

sdk:
	mkdir -p $(SDK_OUTPUT_DIR)
	cp -r sdk/Laniakea.NET.Sdk/* $(SDK_OUTPUT_DIR)/

install: out/shared out/sdk
	rm -rf /usr/share/dotnet/shared/Laniakea.NET
	cp -r out/shared/Laniakea.NET /usr/share/dotnet/shared/
	rm -rf /usr/share/dotnet/sdk/$(DOTNET_SDK_VERSION)/Sdks/Laniakea.NET.Sdk
	cp -r $(SDK_OUTPUT_DIR) /usr/share/dotnet/sdk/$(DOTNET_SDK_VERSION)/Sdks/

uninstall:
	rm -rf /usr/share/dotnet/shared/Laniakea.NET
	rm -rf /usr/share/dotnet/sdk/$(DOTNET_SDK_VERSION)/Sdks/Laniakea.NET.Sdk

install-runtime:
	install -d -m755 $(DESTDIR)/usr/share/dotnet/shared/
	cp -r out/shared/Laniakea.NET $(DESTDIR)/usr/share/dotnet/shared/

install-sdk:
	install -d -m755 $(DESTDIR)/usr/share/dotnet/sdk/$(DOTNET_SDK_VERSION)/Sdks/
	cp -r out/sdk/Sdks/Laniakea.NET.Sdk $(DESTDIR)/usr/share/dotnet/sdk/$(DOTNET_SDK_VERSION)/Sdks/

clean:
	rm -rf out build

.PHONY: runtime sdk
