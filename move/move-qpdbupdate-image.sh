#!/bin/sh
docker pull '||registry-source||/qpdbupdate:||version||' && \
docker tag '||registry-source||/qpdbupdate:||version||' '||registry-destination||/qpdbupdate:||version||' && \
docker push '||registry-destination||/qpdbupdate:||version||'