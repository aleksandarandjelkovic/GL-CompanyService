CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Companies" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Ticker" character varying(10) NOT NULL,
    "Exchange" character varying(20) NOT NULL,
    "ISIN" character varying(12) NOT NULL,
    "Website" character varying(255) NOT NULL,
    CONSTRAINT "PK_Companies" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ISIN_Format" CHECK (ISIN ~ '^[A-Z]{2}[A-Z0-9]{9}[0-9]$')
);

CREATE UNIQUE INDEX "IX_Companies_ISIN" ON "Companies" ("ISIN");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250612100623_InitialCreate', '9.0.6');

COMMIT;

