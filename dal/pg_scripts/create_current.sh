rm -rf current.sql
cat current.txt | 
while read folder; do find $folder -name "*.sql" | 
sort -f -k 1,1 -t . ; done | 
while read file; do cat "$file" >> current.sql && echo "" >> current.sql; done  
