CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "Groups" (
    "Id" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Groups" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Groups_Users_CreatedBy" FOREIGN KEY ("CreatedBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "GroupMembers" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "GroupId" uuid NOT NULL,
    CONSTRAINT "PK_GroupMembers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_GroupMembers_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupMembers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_GroupMembers_GroupId" ON "GroupMembers" ("GroupId");

CREATE UNIQUE INDEX "IX_GroupMembers_UserId_GroupId" ON "GroupMembers" ("UserId", "GroupId");

CREATE INDEX "IX_Groups_CreatedBy" ON "Groups" ("CreatedBy");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260328130616_InitialCreate', '10.0.5');

COMMIT;

START TRANSACTION;
CREATE TABLE "Expenses" (
    "Id" uuid NOT NULL,
    "GroupId" uuid NOT NULL,
    "PaidBy" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Description" character varying(500) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Expenses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Expenses_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Expenses_Users_PaidBy" FOREIGN KEY ("PaidBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ExpenseSplits" (
    "Id" uuid NOT NULL,
    "ExpenseId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    CONSTRAINT "PK_ExpenseSplits" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ExpenseSplits_Expenses_ExpenseId" FOREIGN KEY ("ExpenseId") REFERENCES "Expenses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ExpenseSplits_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Expenses_GroupId" ON "Expenses" ("GroupId");

CREATE INDEX "IX_Expenses_PaidBy" ON "Expenses" ("PaidBy");

CREATE UNIQUE INDEX "IX_ExpenseSplits_ExpenseId_UserId" ON "ExpenseSplits" ("ExpenseId", "UserId");

CREATE INDEX "IX_ExpenseSplits_UserId" ON "ExpenseSplits" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260328131747_AddExpenseManagement', '10.0.5');

COMMIT;

START TRANSACTION;
CREATE TABLE "GroupJoinRequests" (
    "Id" uuid NOT NULL,
    "GroupId" uuid NOT NULL,
    "RequestedByUserId" uuid NOT NULL,
    "TargetUserId" uuid NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RespondedAt" timestamp with time zone,
    CONSTRAINT "PK_GroupJoinRequests" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_GroupJoinRequests_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupJoinRequests_Users_RequestedByUserId" FOREIGN KEY ("RequestedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupJoinRequests_Users_TargetUserId" FOREIGN KEY ("TargetUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_GroupJoinRequests_GroupId_TargetUserId_Status" ON "GroupJoinRequests" ("GroupId", "TargetUserId", "Status");

CREATE INDEX "IX_GroupJoinRequests_RequestedByUserId" ON "GroupJoinRequests" ("RequestedByUserId");

CREATE INDEX "IX_GroupJoinRequests_TargetUserId" ON "GroupJoinRequests" ("TargetUserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260328162326_AddGroupJoinRequests', '10.0.5');

COMMIT;

START TRANSACTION;
ALTER TABLE "Expenses" ADD "Notes" character varying(1000) NOT NULL DEFAULT '';

CREATE TABLE "ExpensePayments" (
    "Id" uuid NOT NULL,
    "ExpenseId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ExpensePayments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ExpensePayments_Expenses_ExpenseId" FOREIGN KEY ("ExpenseId") REFERENCES "Expenses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ExpensePayments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_ExpensePayments_ExpenseId_UserId" ON "ExpensePayments" ("ExpenseId", "UserId");

CREATE INDEX "IX_ExpensePayments_UserId" ON "ExpensePayments" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260328165035_AddExpensePaymentsAndNotes', '10.0.5');

COMMIT;


