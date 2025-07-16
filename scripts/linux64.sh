set -e

cd ../src

echo "Compiling TheCult.csproj..."
dotnet publish TheCult.csproj -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true

echo "Moving release..."
mkdir -p ../release/linux/
cp -r bin/Release/net9.0/linux-x64/publish/TheCult ../release/linux/

echo "All done!"
