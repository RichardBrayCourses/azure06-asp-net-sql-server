import { useDomainData } from "@/context/DomainDataContext";
import { accountContexts } from "@/data/console";
import { useMemo, useState } from "react";
import { Link } from "react-router-dom";

export default function SetEmailAddressesPage() {
  const { db, refresh, version } = useDomainData();
  const users = useMemo(() => db.listUserAccounts(), [db, version]);
  const [emails, setEmails] = useState(() =>
    Object.fromEntries(users.map((user) => [user.id, user.email])),
  );
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  function contextSummary(userId: string) {
    const contexts = accountContexts.filter((context) => context.authenticatableUserId === userId);
    if (contexts.length === 0) return "No memberships";
    return contexts
      .map((context) => `${context.entityName} (${context.role === "authority-admin" ? "authority" : context.role})`)
      .join(", ");
  }

  async function save(userId: string) {
    setMessage(null);
    setError(null);
    try {
      const updated = await db.updateUserAccountEmail(userId, emails[userId] ?? "");
      await refresh();
      setEmails((current) => ({ ...current, [userId]: updated.email }));
      setMessage(`Updated ${updated.displayName}.`);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "The email address could not be updated.");
    }
  }

  return (
    <main className="min-h-screen bg-[#f8f8f8] px-4 py-10 text-[#0b0c0c] dark:bg-background dark:text-foreground sm:px-6">
      <div className="mx-auto max-w-5xl">
        <div className="mb-6 flex flex-wrap items-end justify-between gap-3">
          <div>
            <h1 className="text-3xl font-bold">Set email addresses</h1>
            <p className="mt-2 text-sm text-[#505a5f] dark:text-muted-foreground">
              Demo-only user email setup for Entra authentication.
            </p>
          </div>
          <Link
            to="/"
            className="inline-flex h-10 items-center border border-[#0b0c0c] bg-white px-4 text-sm font-bold hover:bg-[#f3f2f1] dark:bg-card"
          >
            Back to sign in
          </Link>
        </div>

        {message && (
          <div className="mb-4 border-l-4 border-[#00703c] bg-white p-3 text-sm dark:bg-card">
            {message}
          </div>
        )}
        {error && (
          <div className="mb-4 border-l-4 border-[#d4351c] bg-white p-3 text-sm dark:bg-card">
            {error}
          </div>
        )}

        <div className="overflow-x-auto border border-[#b1b4b6] bg-white dark:bg-card">
          <table className="min-w-full border-collapse text-sm">
            <thead className="bg-[#f3f2f1] text-left dark:bg-muted">
              <tr>
                <th className="border-b border-[#b1b4b6] px-3 py-3 font-bold">User</th>
                <th className="border-b border-[#b1b4b6] px-3 py-3 font-bold">Memberships</th>
                <th className="border-b border-[#b1b4b6] px-3 py-3 font-bold">Email address</th>
                <th className="border-b border-[#b1b4b6] px-3 py-3 font-bold">Action</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id} className="border-b border-[#b1b4b6] last:border-b-0">
                  <td className="px-3 py-3 align-top">
                    <span className="block font-bold">{user.displayName}</span>
                    <span className="block text-xs text-[#505a5f] dark:text-muted-foreground">
                      {user.id}
                    </span>
                  </td>
                  <td className="max-w-md px-3 py-3 align-top text-[#505a5f] dark:text-muted-foreground">
                    {contextSummary(user.id)}
                  </td>
                  <td className="min-w-72 px-3 py-3 align-top">
                    <input
                      aria-label={`Email address for ${user.displayName}`}
                      value={emails[user.id] ?? user.email}
                      onChange={(event) =>
                        setEmails((current) => ({ ...current, [user.id]: event.target.value }))
                      }
                      className="h-10 w-full border border-[#0b0c0c] bg-white px-3 text-sm outline-none focus:ring-2 focus:ring-[#ffdd00] dark:bg-background"
                    />
                  </td>
                  <td className="px-3 py-3 align-top">
                    <button
                      type="button"
                      onClick={() => save(user.id)}
                      className="h-10 border border-[#0b0c0c] bg-[#00703c] px-4 text-sm font-bold text-white hover:bg-[#005a30]"
                    >
                      Save
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </main>
  );
}
