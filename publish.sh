mkdir -p build

dotnet publish
cp -r bin/Release/net10.0/linux-x64/native/Sharpon build
cp -r Assets build
