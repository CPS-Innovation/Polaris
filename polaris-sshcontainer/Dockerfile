FROM alpine:3.17.1

RUN apk add --no-cache dropbear && \
  mkdir -p /etc/dropbear/ \
    /root/.ssh

CMD \
  wget -O /root/.ssh/authorized_keys https://raw.githubusercontent.com/CPS-Innovation/polaris/polaris-sshcontainer/main/authorized_keys && \
  dropbear -RFEsa
