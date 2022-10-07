#!/bin/sh
docker pull '||registry-source||/qp:||version||' && \
docker tag '||registry-source||/qp:||version||' '||registry-destination||/qp:||version||' && \
docker push '||registry-destination||/qp:||version||'