ARG image_debian
FROM ${image_debian}
ARG CodeUnitVersion
ARG DotNetRuntime
LABEL version=${CodeUnitVersion}
ENV ISRUNNINGINCONTAINER="true"
#TODO make it possible to have an own apt-server for apt-packages to get rid of rate limits
RUN mkdir /Workspace && \
    mkdir /Workspace/Application && \
    mkdir /Workspace/Application/Backend && \
    mkdir /Workspace/Application/Frontend && \
    mkdir /Workspace/Application/Frontend/WebApplicationFiles && \
    mkdir /Workspace/Configuration && \
    mkdir /Workspace/Logs && \
    mkdir /Workspace/Other && \
    mkdir /Workspace/Other/Certificates && \
    mkdir /Workspace/Other/EntryPoint && \
    apt-get update && \
    apt-get install -y curl nginx wget

WORKDIR /Workspace/Other/EntryPoint

RUN wget https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y aspnetcore-runtime-10.0

RUN rm -rf /var/lib/apt/lists/*

ADD Other/Resources/DependentCodeUnits/OpenIMSBackend/BuildResult_DotNet_${DotNetRuntime} /Workspace/Application/Backend
ADD Other/Resources/DependentCodeUnits/OpenIMSFrontend/BuildResult_WebApplication /Workspace/Application/Frontend/WebApplicationFiles
COPY Other/Resources/DevelopmentCertificate/OpenIMSDevelopmentCertificate.crt /Workspace/Other/Certificates
COPY Other/Resources/DevelopmentCertificate/OpenIMSDevelopmentCertificate.key /Workspace/Other/Certificates
COPY OpenIMS/nginx.conf /Workspace/Application/Frontend
COPY OpenIMS/nginx.loadings-settings.conf /Workspace/Application/Frontend


COPY OpenIMS/EntryPoint.sh /Workspace/Other/EntryPoint
RUN chmod +x /Workspace/Other/EntryPoint/EntryPoint.sh
COPY OpenIMS/HealthCheck.sh /Workspace/Other/EntryPoint
RUN chmod +x /Workspace/Other/EntryPoint/HealthCheck.sh

HEALTHCHECK --interval=30s --timeout=20s --start-period=2m CMD ./HealthCheck.sh

ENTRYPOINT ["./EntryPoint.sh"]
