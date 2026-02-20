SHARPON_DIR="/media/SDL3/Sharpon/Sharpon"

echo Deleting all previous builds...
rm -rf "$SHARPON_DIR"/bin/Release/net10.0/*

echo Building new versions...

cd $SHARPON_DIR
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true

echo Deleting libSDL3.so from linux build

rm $SHARPON_DIR/bin/Release/net10.0/linux-x64/publish/libSDL3.so

echo Copying the right libSDL3 files into the linux build

cp /media/SDL3/libSDL3-build/*.so $SHARPON_DIR/bin/Release/net10.0/linux-x64/publish/

echo Zipping publish folders...

cd $SHARPON_DIR/bin/Release/net10.0/linux-x64
zip -r $SHARPON_DIR/bin/Release/net10.0/Sharpon-Linux.zip ./publish/

cd ../win-x64
zip -r $SHARPON_DIR/bin/Release/net10.0/Sharpon-Windows.zip ./publish/


