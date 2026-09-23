mkdir -p build/Linux

dotnet publish
cp bin/Release/net10.0/linux-x64/native/Sharpon build/Linux
cp -r Assets build/Linux
