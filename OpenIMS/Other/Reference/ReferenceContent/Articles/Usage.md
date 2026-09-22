# Usage

## Access

The container listens on port 8080 (HTTP) and 443 (HTTPS).
Even if internally nginx is used as internal reverse-proxy to distinguish between requests for frontend and backend this is not meant as reverse-proxy when the reference says it is supposed to be hosted as reverse-proxy.
The container itself only contains the development-certificate to that it is able to be hosted using HTTPS.
Anyway, this is not a public trusted certificate.
There is also no mechanism to retrieve a public trusted certificate.
Treat the OpenIMS-container as container which exposes its functionality without TLS.
For this reason you have to setup an own reverse-proxy which is providing a proper HTTPS-endpoint.
This reverse-proxy can then forward the entire OpenIMS-traffic (for frontend and backend) to either http://container-address:8080 or https://container-address:443.
OpenIMS provides both-endpoints for the following reasons:

- 8080 (HTTP): Can be used as forward-target by your reverse-proxy because it is a little bit more performant (due to missing second TLS-termination) for productive usage. 
- 443 (HTTPS): Can be used to test the entire app on a development-machine in the context of the `BaseExampleStart`-task: It can be verified that the entire frontend-/backend-communication is working without [CORS](https://developer.mozilla.org/de/docs/Web/HTTP/Guides/CORS)-issues or something similar without the requirement of setting up a reverse-proxy manually.

## First login

On the very first start OpenIMS creates the user `admin`.
Its password is the value of the environment-variable `InitialAdminPassword`; if that variable is not set then the password is `admin`.

The password is only used when the account is created, so changing the variable later has no effect.

Further user-accounts are created by an administrator in the admin-area of the web-frontend.
