# Certificate Generation Workflows for FreeIPA, CFSSL, and .NET Services

This document summarizes the certificate generation workflows for all servers and services in this solution.

---

## Overview

Certificates are required for secure mTLS communication between all microservices, clients, and servers.  
This guide covers how to generate and sign certificates for:

- Internal clients and servers (using FreeIPA)
- External clients and servers (using CFSSL)
- .NET API Server and Web Server

---

## 1. FreeIPA Certificate Generation (Internal Trust)

### 1.1 Generate Private Key and CSR

```bash
openssl req -new -newkey rsa:2048 -nodes \
  -keyout infrastructure/temp/<name>-key.pem \
  -out infrastructure/temp/<name>.csr \
  -subj "/CN=<name>"
```

### 1.2 Copy CSR to FreeIPA Container

```bash
docker cp infrastructure/temp/<name>.csr freeipa:/tmp/<name>.csr
```

### 1.3 Sign CSR in FreeIPA

```bash
docker exec -it freeipa bash
kinit admin
ipa cert-request /tmp/<name>.csr --principal=<principal> --certificate-out=/tmp/<name>.pem
```

### 1.4 Retrieve Signed Certificate

```bash
docker cp freeipa:/tmp/<name>.pem src/CertificateDemo/<TargetProject>/certs/<name>.pem
cp infrastructure/temp/<name>-key.pem src/CertificateDemo/<TargetProject>/certs/<name>-key.pem
```

---

## 2. CFSSL Certificate Generation (External Trust)

### 2.1 Create CSR JSON

```bash
cat > infrastructure/temp/<name>-csr.json <<EOF
{
  "CN": "<name>.acme.com",
  "hosts": ["<name>.acme.com"],
  "key": {"algo": "rsa", "size": 2048},
  "names": [{"C": "DE", "O": "ACME"}]
}
EOF
```

### 2.2 Generate Private Key and CSR

```bash
cfssl genkey infrastructure/temp/<name>-csr.json | cfssljson -bare infrastructure/temp/<name>
```

### 2.3 Sign CSR with CFSSL

```bash
cd infrastructure/cfssl/config
cfssl sign -ca ca.pem -ca-key ca-key.pem -config config.json -profile external ../../temp/<name>.csr | cfssljson -bare ../../temp/<name>
```

### 2.4 Copy Certificate and Key

```bash
cp infrastructure/temp/<name>.pem src/CertificateDemo/<TargetProject>/certs/<name>.pem
cp infrastructure/temp/<name>-key.pem src/CertificateDemo/<TargetProject>/certs/<name>-key.pem
```

---

## 3. Exporting Client Certificates for Browser Use

### 3.1 Export as PKCS#12

```bash
openssl pkcs12 -export \
  -in src/CertificateDemo/Clients.ExternalClient/certs/client-external.pem \
  -inkey src/CertificateDemo/Clients.ExternalClient/certs/client-external-key.pem \
  -out client-external.p12 \
  -name "external-client"
```

### 3.2 Import into Browser

- Chrome/Edge: Settings → Privacy and Security → Security → Manage Certificates
- Firefox: Settings → Privacy & Security → Certificates → View Certificates
- Safari: Use Keychain Access

---

## 4. Notes

- Always use the correct CA and profile for signing (FreeIPA for internal, CFSSL for external).
- Place generated certificates and keys in the appropriate `certs/` folder for each service/client.
- For production, secure all private keys and CA credentials.
