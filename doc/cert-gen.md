# Certificate Generation Workflows for FreeIPA, CFSSL, and .NET Services

This guide walks you through **end‑to‑end certificate lifecycle management** for secure mTLS communication between microservices, clients, and servers. It covers:

1. One‑time FreeIPA server initialization  
2. FreeIPA certificate generation (internal trust)  
3. CFSSL certificate generation (external trust)  
4. Exporting client certificates for browser use  

---

## 0. One‑Time FreeIPA Server Initialization

Before you can sign any CSRs, FreeIPA needs principals (users or hosts) defined in its directory. Perform these steps **once** after your container is up:

```bash
# a) Enter the FreeIPA container shell
docker exec -it freeipa bash

# b) Authenticate as admin
kinit admin

# c) Create a user principal for your internal client
ipa user-add client1 \
    --first=John --last=Doe \
    --password
# (you’ll be prompted to set the password)

# d) (Optional) For host‑style certs: create host principal + DNS record
ipa host-add host1.internal.acme.com \
    --ip-address=192.168.XXX.YYY

ipa dnsrecord-add internal.acme.com host1 \
    --a-rec=192.168.XXX.YYY

# e) (Optional) Inspect available cert profiles
ipa certprofile-show caIPAservice

# f) Exit container
exit
````

> **Why this matters:** FreeIPA will only sign CSRs whose `--principal` matches an existing directory entry. By adding users/hosts up front and (for hosts) the corresponding DNS records, you ensure your CSRs are accepted.

---

## 1. FreeIPA Certificate Generation (Internal Trust)

This workflow issues client or host certificates from the FreeIPA CA.

### 1.1 Generate Private Key & CSR

```bash
openssl req -new -newkey rsa:2048 -nodes \
  -keyout infrastructure/temp/<name>-key.pem \
  -out infrastructure/temp/<name>.csr \
  -subj "/CN=<name>"
```

* Replace `<name>` with the principal name you created above (e.g. `client1` or `host1.internal.acme.com`).

### 1.2 Copy CSR into the FreeIPA Container

```bash
docker cp infrastructure/temp/<name>.csr freeipa:/tmp/<name>.csr
```

### 1.3 Sign CSR in FreeIPA

```bash
docker exec -it freeipa bash -c "\
  kinit admin && \
  ipa cert-request /tmp/<name>.csr \
    --principal=<principal> \
    --certificate-out=/tmp/<name>.pem"
```

* **`<principal>`** must exactly match the Kerberos principal you created:

  * For user certs: `client1@INTERNAL.ACME.COM`
  * For host certs: `host/host1.internal.acme.com@INTERNAL.ACME.COM`

### 1.4 Retrieve Signed Certificate

```bash
docker cp freeipa:/tmp/<name>.pem infrastructure/temp/<name>.pem
cp infrastructure/temp/<name>-key.pem \
   src/CertificateDemo/<TargetProject>/certs/<name>-key.pem

mv infrastructure/temp/<name>.pem \
   src/CertificateDemo/<TargetProject>/certs/<name>.pem
```

> **Tip:** Keep your `.pem` and `-key.pem` files together under the target project’s `certs/` folder for easy reference in your .NET code.

---

## 2. CFSSL Certificate Generation (External Trust)

Use CFSSL for external clients or servers (third‑party, browser, etc.).

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

### 2.2 Generate Private Key & CSR

```bash
cfssl genkey infrastructure/temp/<name>-csr.json | \
  cfssljson -bare infrastructure/temp/<name>
```

### 2.3 Sign CSR with CFSSL

```bash
cd infrastructure/cfssl/config
cfssl sign \
  -ca ca.pem \
  -ca-key ca-key.pem \
  -config config.json \
  -profile external \
  ../../temp/<name>.csr | \
  cfssljson -bare ../../temp/<name>
```

### 2.4 Copy Certificate & Key

```bash
cp infrastructure/temp/<name>.pem \
   src/CertificateDemo/<TargetProject>/certs/<name>.pem
cp infrastructure/temp/<name>-key.pem \
   src/CertificateDemo/<TargetProject>/certs/<name>-key.pem
```

---

## 3. Exporting Client Certificates for Browser Use

To present client certs to browsers, bundle them as PKCS#12:

```bash
openssl pkcs12 -export \
  -in src/CertificateDemo/Clients.ExternalClient/certs/client-external.pem \
  -inkey src/CertificateDemo/Clients.ExternalClient/certs/client-external-key.pem \
  -out client-external.p12 \
  -name "external-client"
```

* **Import** into your browser’s certificate store:

  * **Chrome/Edge:** Settings → Privacy & Security → Manage Certificates
  * **Firefox:** Settings → Privacy & Security → View Certificates
  * **Safari:** Keychain Access

---

## 4. Final Notes & Troubleshooting

* **Principal must exist**: FreeIPA won’t sign unknown principals.
* **CSR names must match**: The CSR’s CN or SAN must include the principal you pass to `--principal`.
* **Clock sync**: Kerberos is time‑sensitive; ensure host and container clocks match.
* **Profiles**: Use `ipa certprofile-show` to verify which profiles (e.g. `caIPAservice`) are available.
* **.NET integration**: Place your certs under each project’s `certs/` folder and configure Kestrel to load them for mTLS.
