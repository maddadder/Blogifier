FROM alpine:3.16
RUN apk add --no-cache postgresql-client bash

COPY ./src/cmd.sh /cmd.sh
CMD ["sh", "./cmd.sh"]