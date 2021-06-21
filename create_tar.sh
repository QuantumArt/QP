find . -name "*.csproj" -print0 | tar -cvf projectfiles.tar --mtime='1970-01-01' --null -T -
