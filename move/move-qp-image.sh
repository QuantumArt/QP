#!/bin/sh
docker pull '||registry-source||/||image-source||:||version||' && \
docker tag '||registry-source||/||image-source||:||version||' '||registry-destination||/||image-destination||:||version||' && \
docker push '||registry-destination||/||image-destination||:||version||'