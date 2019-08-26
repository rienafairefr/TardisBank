FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# copy solution and restore as distinct layers
COPY src/ ./
RUN dotnet restore

# run the unit tests
ENV TARDISBANK_DB_CON='Host=127.0.0.1;Port=5432;Database=tardisbank;User Id=some_user;Password=some_password;'
ENV TARDISBANK_KEY='somekey'
ENV TARDISBANK_SMTP_SERVER=':25:false'
ENV TARDISBANK_SMTP_CREDENTIALS='test:test'

CMD ["dotnet", "watch", "run"]