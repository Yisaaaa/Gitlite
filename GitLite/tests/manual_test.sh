BUILD="/home/luis/RiderProjects/GitLite/GitLite/bin/Release/net6.0/linux-x64/publish/GitLite"
cd ../
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
cd ~/gitlite
rm -rf .gitlite
cp $BUILD ./