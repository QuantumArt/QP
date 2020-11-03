rm -f current.sql
cat current.txt | sed -e 's/\r//g' | while read folder; do find $folder -name "*.sql" | LC_COLLATE='C.UTF-8' sort -f; done | while read file; do cat "$file" >> current.sql && echo "" >> current.sql; done  
