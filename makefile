define \n


endef

PLATFORMS := win-x64 linux-x64 osx-x64
default:
	$(foreach rid,$(PLATFORMS),dotnet publish -r $(rid) -c Release -p:PublishSingleFile=true ${\n})
	