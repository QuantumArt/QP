#!/bin/sh
docker pull '||registry-source||/||update-image-source||:||version||' && \
docker tag '||registry-source||/||update-image-source||:||version||' '||registry-destination||/||update-image-destination||:||version||' && \
docker push '||registry-destination||/||update-image-destination||:||version||'