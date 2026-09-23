mkdir -p build

dotnet publish
cp bin/Release/net10.0/linux-x64/native/Sharpon build
cp -r Assets build
