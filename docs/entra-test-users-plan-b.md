# Entra Test Users With onmicrosoft.com Accounts

## Purpose

This document explains how to create test sign-ins for the testing system without using Gmail and without using Gmail `+` aliases.

The plan is to create Microsoft Entra users inside your own Microsoft Entra tenant. These users will have sign-in names ending in your tenant's `onmicrosoft.com` domain.

## Terms Used In This Document

A Microsoft Entra tenant is the Microsoft identity directory that contains users, app registrations, and sign-in settings for this project.

An `onmicrosoft.com` domain is the default domain that Microsoft gives to a Microsoft Entra tenant. It usually looks like this:

```text
yourtenant.onmicrosoft.com
```

A user principal name is the username that a Microsoft Entra user types when signing in. It often looks like an email address. For these test users, it will look like this:

```text
jonathan.price@yourtenant.onmicrosoft.com
```

An object ID is the stable unique ID that Microsoft Entra assigns to a user. It is not the user's email address. It is usually a GUID, which is a long ID shaped like this:

```text
11111111-2222-3333-4444-555555555555
```

The application database is the SQL database used by this project. It has a `UserAccounts` table. Each row in that table represents one application user, such as Jonathan Price.

The `EntraObjectId` column is the column in the application database that connects an application user to a Microsoft Entra user. When someone signs in, Microsoft Entra sends the user's object ID to the application. The application then looks for a matching `EntraObjectId` value in the `UserAccounts` table.

## Why Gmail Plus Aliases Do Not Help Here

Gmail `+` aliases only change where email is delivered. For example, these two addresses can both deliver to the same Gmail inbox:

```text
arty.uptick@gmail.com
arty.uptick+jonathan.price@gmail.com
```

Microsoft Entra login does not treat a Gmail `+` alias as a Microsoft Entra user. The alias can receive email, but it is not automatically a sign-in account.

For this project, the important value is the Microsoft Entra object ID, not the email address.

## The Plan

Create one Microsoft Entra user for each seeded application user.

For example, the seeded application user Jonathan Price can become this Microsoft Entra user:

```text
jonathan.price@yourtenant.onmicrosoft.com
```

After the Microsoft Entra user is created, copy that user's object ID into the Jonathan Price row in the application database.

When Jonathan signs in with:

```text
jonathan.price@yourtenant.onmicrosoft.com
```

Microsoft Entra sends Jonathan's object ID to the application. The application finds the `UserAccounts` row with the same `EntraObjectId`. Jonathan is then treated as the seeded Jonathan Price user.

## Step 1: Find Your onmicrosoft.com Domain

Open the Microsoft Entra admin center.

Go to:

```text
Identity -> Settings -> Domain names
```

Find the default domain ending in:

```text
onmicrosoft.com
```

Write it down. In the examples below, this document uses:

```text
yourtenant.onmicrosoft.com
```

Replace that example with your real domain.

## Step 2: Create The Test Users

Open the Microsoft Entra admin center.

Go to:

```text
Identity -> Users -> All users -> New user -> Create new user
```

Create one user for each test persona.

For Jonathan Price, use values like these:

```text
User principal name: jonathan.price@yourtenant.onmicrosoft.com
Display name: Jonathan Price
Password: let Microsoft generate a temporary password
Account enabled: yes
```

Repeat this for each seeded user.

Use predictable names. For example:

```text
jonathan.price@yourtenant.onmicrosoft.com
amara.singh@yourtenant.onmicrosoft.com
aisha.khan@yourtenant.onmicrosoft.com
michael.reeves@yourtenant.onmicrosoft.com
```

Keep a temporary note of each username and temporary password. Treat that note as sensitive, because anyone with those details can try to sign in as the test user.

## Step 3: Copy Each User's Object ID

For each Microsoft Entra user you created, open the user in the Microsoft Entra admin center.

Find the field named:

```text
Object ID
```

Copy it.

Create a small mapping table for yourself. It should look like this:

```text
Seeded application user | Sign-in username | Microsoft Entra object ID
Jonathan Price | jonathan.price@yourtenant.onmicrosoft.com | 11111111-2222-3333-4444-555555555555
Amara Singh | amara.singh@yourtenant.onmicrosoft.com | 66666666-7777-8888-9999-000000000000
```

## Step 4: Update The Application Database

The application database must use the real Microsoft Entra object IDs.

The seeded data currently contains placeholder values like this:

```text
entra-user-jonathan-price
```

Those placeholder values work in automated tests, but they do not work with real Microsoft Entra login.

Replace each placeholder `EntraObjectId` with the real Microsoft Entra object ID for that user.

For Jonathan Price, the application database row should change from this:

```text
UserAccounts.Id: user-jonathan-price
UserAccounts.EntraObjectId: entra-user-jonathan-price
```

to this:

```text
UserAccounts.Id: user-jonathan-price
UserAccounts.EntraObjectId: 11111111-2222-3333-4444-555555555555
```

Use Jonathan's real object ID, not the example value above.

## Step 5: Sign In As A Test User

Open the testing site:

```text
https://testing.all-checks-out.com
```

Sign in with one of the new Microsoft Entra usernames, such as:

```text
jonathan.price@yourtenant.onmicrosoft.com
```

Use the temporary password from Microsoft Entra.

Microsoft may ask you to change the password on first sign-in. If it does, set a new password and record it somewhere safe.

## What Success Looks Like

The Microsoft login screen accepts the username.

The login returns to the testing site.

The application recognises the signed-in user as the matching seeded application user.

For Jonathan Price, the application should treat the signed-in user as:

```text
Jonathan Price
```

## What To Check If It Fails

If Microsoft says the account does not exist, check that the username was created in the same Microsoft Entra tenant used by the testing application.

If Microsoft accepts the login but the application does not recognise the user, check the `EntraObjectId` value in the application database.

If the application recognises the wrong user, check that the object ID was copied into the correct `UserAccounts` row.

If the user cannot access the expected application data, check that the seeded user's memberships are correct. A membership is the database record that connects a user to a role-like position in the application, such as authority user, participant user, stakeholder user, or agent user.

## Short Version

Create real Microsoft Entra users ending in your tenant's `onmicrosoft.com` domain.

Copy each user's Microsoft Entra object ID.

Put each real object ID into the matching application database user row.

Sign in with the new `onmicrosoft.com` username.
