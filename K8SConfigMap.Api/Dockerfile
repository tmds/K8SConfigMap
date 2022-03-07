FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /app

ARG configuration=Release
ARG version=0.0.0

COPY . ./

RUN rm -rf ./K8SConfigMap.Api/servicemap

RUN dotnet publish ./K8SConfigMap.Api/K8SConfigMap.Api.csproj --configuration ${configuration} -o ./publish \
      --runtime alpine-x64 \
      --self-contained true \
      /p:Version=${version}${suffix} \
      /p:PublishTrimmed=true \
      /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80

COPY --from=build /app/publish /app

ENV \
    BUILD_NUMBER=$build_number \
    BUILD_TIME=$build_time \
    BRANCH=$branch \
    COMMIT=$commit

ENTRYPOINT ["./K8SConfigMap.Api"]