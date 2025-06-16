FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Company.Tests/Company.Tests.csproj", "Company.Tests/"]
COPY ["Company.Api/Company.Api.csproj", "Company.Api/"]
COPY ["Company.Application/Company.Application.csproj", "Company.Application/"]
COPY ["Company.Domain/Company.Domain.csproj", "Company.Domain/"]
COPY ["Company.Infrastructure/Company.Infrastructure.csproj", "Company.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Company.Tests/Company.Tests.csproj"

# Copy the rest of the code
COPY . .

# Create wait-for-it.sh script
RUN echo '#!/bin/sh\n\
\n\
TIMEOUT=15\n\
QUIET=0\n\
\n\
echoerr() {\n\
  if [ "$QUIET" -ne 1 ]; then printf "%s\\n" "$*" 1>&2; fi\n\
}\n\
\n\
usage() {\n\
  exitcode="$1"\n\
  cat << USAGE >&2\n\
Usage:\n\
  $cmdname host:port [-t timeout] [-- command args]\n\
  -q | --quiet                        Do not output any status messages\n\
  -t TIMEOUT | --timeout=timeout      Timeout in seconds, zero for no timeout\n\
  -- COMMAND ARGS                     Execute command with args after the test finishes\n\
USAGE\n\
  exit "$exitcode"\n\
}\n\
\n\
wait_for() {\n\
  for i in `seq $TIMEOUT` ; do\n\
    nc -z "$HOST" "$PORT" > /dev/null 2>&1\n\
    \n\
    result=$?\n\
    if [ $result -eq 0 ] ; then\n\
      if [ $# -gt 0 ] ; then\n\
        exec "$@"\n\
      fi\n\
      exit 0\n\
    fi\n\
    sleep 1\n\
  done\n\
  echo "Operation timed out" >&2\n\
  exit 1\n\
}\n\
\n\
while [ $# -gt 0 ]\n\
do\n\
  case "$1" in\n\
    *:* )\n\
    HOST=$(printf "%s\\n" "$1"| cut -d : -f 1)\n\
    PORT=$(printf "%s\\n" "$1"| cut -d : -f 2)\n\
    shift 1\n\
    ;;\n\
    -q | --quiet)\n\
    QUIET=1\n\
    shift 1\n\
    ;;\n\
    -t)\n\
    TIMEOUT="$2"\n\
    if [ -z "$TIMEOUT" ] ; then break; fi\n\
    shift 2\n\
    ;;\n\
    --timeout=*)\n\
    TIMEOUT="${1#*=}"\n\
    shift 1\n\
    ;;\n\
    --)\n\
    shift\n\
    break\n\
    ;;\n\
    *)\n\
    echoerr "Unknown argument: $1"\n\
    usage 1\n\
    ;;\n\
  esac\n\
done\n\
\n\
if [ "$HOST" = "" -o "$PORT" = "" ]; then\n\
  echoerr "Error: you need to provide a host and port to test."\n\
  usage 2\n\
fi\n\
\n\
wait_for "$@"\n\
' > /src/wait-for-it.sh && chmod +x /src/wait-for-it.sh

# Install netcat for wait-for-it.sh
RUN apt-get update && apt-get install -y netcat-openbsd

# Update test appsettings for Docker environment
RUN sed -i 's/localhost/postgres/g' Company.Tests/appsettings.Test.json

# Create a wait and run script
RUN echo '#!/bin/sh\n\
echo "Waiting for PostgreSQL..."\n\
/src/wait-for-it.sh postgres:5432 -t 30\n\
echo "Waiting for API..."\n\
/src/wait-for-it.sh api:5000 -t 30\n\
echo "Waiting for Auth Server..."\n\
/src/wait-for-it.sh company-auth:5000 -t 30\n\
echo "All dependencies are ready. Running tests..."\n\
dotnet test Company.Tests/Company.Tests.csproj --logger:console\;verbosity=detailed\n\
' > /src/run-tests.sh && chmod +x /src/run-tests.sh

# Set entrypoint to run the wait and test script
ENTRYPOINT ["/src/run-tests.sh"] 