import { useAuth, type UserRole } from "@/context/AuthContext";
import {
  terminologyLabel,
  terminologyTitle,
  type AccountContext,
  type AuthorityDto,
  type UserAccountDto,
} from "@/data/console";
import {
  loadDemoSignInOptions,
  type DemoSignInMembershipDto,
  type DemoSignInMembershipType,
  type DemoSignInOptionsDto,
} from "@/data/demoSignIn";
import { useEffect, useMemo, useState } from "react";

type PrimaryContextType = DemoSignInMembershipType;
type EntityOption = {
  id: string;
  name: string;
  authorityName?: string;
};
type RuntimeAccountContext = Omit<AccountContext, "entityId" | "entityType" | "role"> & {
  entityType: PrimaryContextType;
  entityId: string;
  role: UserRole;
  entraObjectId: string;
};

export default function SignInPage() {
  const { login } = useAuth();
  const [options, setOptions] = useState<DemoSignInOptionsDto | null>(null);
  const [selectedContextType, setSelectedContextType] = useState<PrimaryContextType | "">("");
  const [selectedEntityId, setSelectedEntityId] = useState("");
  const [signInError, setSignInError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isCurrent = true;
    setIsLoading(true);
    loadDemoSignInOptions()
      .then((nextOptions) => {
        if (isCurrent) {
          setOptions(nextOptions);
          setSignInError(null);
        }
      })
      .catch((caught: unknown) => {
        if (isCurrent) {
          setSignInError(caught instanceof Error ? caught.message : "Unable to load demo sign-in users.");
        }
      })
      .finally(() => {
        if (isCurrent) setIsLoading(false);
      });

    return () => {
      isCurrent = false;
    };
  }, []);

  const selectedMembership = selectedContextType && selectedEntityId
    ? { entityType: selectedContextType, entityId: selectedEntityId }
    : null;
  const terminology = undefined;
  const contextTypeOptions: Array<{ value: PrimaryContextType; label: string }> = [
    { value: "authority", label: terminologyTitle(terminology, "authority") },
    { value: "participant", label: terminologyTitle(terminology, "participant") },
    { value: "stakeholder", label: terminologyTitle(terminology, "stakeholder") },
    { value: "agent", label: terminologyTitle(terminology, "agent") },
  ];
  const entityOptions = useMemo<EntityOption[]>(() => {
    if (!options) return [];
    if (selectedContextType === "authority") {
      return options.authorities.map((authority) => ({ id: authority.id, name: authority.name }));
    }
    if (selectedContextType === "participant") {
      return options.participants.map((participant) => ({
        id: participant.id,
        name: participant.displayName,
        authorityName: findAuthority(options.authorities, participant.authorityId)?.name,
      }));
    }
    if (selectedContextType === "stakeholder") {
      return options.stakeholders.map((stakeholder) => ({
        id: stakeholder.id,
        name: stakeholder.displayName,
        authorityName: findAuthority(options.authorities, stakeholder.authorityId)?.name,
      }));
    }
    if (selectedContextType === "agent") {
      return options.agents.map((agent) => ({
        id: agent.id,
        name: agent.displayName,
        authorityName: findAuthority(options.authorities, agent.authorityId)?.name,
      }));
    }
    return [];
  }, [options, selectedContextType]);
  const accountContexts = useMemo(() => options ? buildAccountContexts(options, terminology) : [], [options, terminology]);
  const filteredUsers = useMemo(() => {
    if (!options || !selectedMembership) return [];
    const userIds = new Set(
      options.memberships
        .filter((membership) => membership.type === selectedMembership.entityType && membership.entityId === selectedMembership.entityId)
        .map((membership) => membership.userAccountId),
    );
    return options.users.filter((identity) => userIds.has(identity.id));
  }, [options, selectedMembership]);

  useEffect(() => {
    setSelectedEntityId("");
  }, [selectedContextType]);

  async function submit(context: RuntimeAccountContext) {
    setSignInError(null);
    try {
      await login({
        authenticatableUserId: context.authenticatableUserId,
        entraObjectId: context.entraObjectId,
        name: context.name,
        email: context.email,
        authorityId: context.authorityId,
        role: context.role,
        accountContextId: context.id,
        accountContextType: context.entityType,
        accountContextEntityId: context.entityId,
        accountContextName: context.entityName,
        participantId: context.participantId,
        stakeholderId: context.stakeholderId,
      });
    } catch (caught) {
      setSignInError(caught instanceof Error ? caught.message : "Unable to start Entra sign in.");
    }
  }

  function getSelectedContextForUser(userId: string) {
    if (!selectedMembership) return undefined;
    return accountContexts.find(
      (context) =>
        context.authenticatableUserId === userId &&
        context.entityType === selectedMembership.entityType &&
        context.entityId === selectedMembership.entityId,
    );
  }

  return (
    <main className="min-h-screen bg-[#f8f8f8] px-4 py-10 text-[#0b0c0c] dark:bg-background dark:text-foreground sm:px-6">
      <div className="mx-auto max-w-xl">
        <div className="mb-6">
          <h1 className="text-3xl font-bold">Sign in</h1>
        </div>

        <section>
          {signInError && (
            <div className="mb-4 border-l-4 border-[#d4351c] bg-white p-3 text-sm dark:bg-card">
              {signInError}
            </div>
          )}

          <div className="grid gap-4">
            <label className="grid gap-1 text-sm font-bold" htmlFor="context-type">
              Account type
              <select
                id="context-type"
                required
                value={selectedContextType}
                disabled={isLoading || !options}
                onChange={(event) => setSelectedContextType(event.target.value as PrimaryContextType | "")}
                className="h-10 w-full border border-[#0b0c0c] bg-white px-3 text-sm font-normal text-[#0b0c0c] outline-none focus:ring-2 focus:ring-[#ffdd00] disabled:border-[#b1b4b6] disabled:bg-[#f3f2f1] disabled:text-[#505a5f] dark:bg-background dark:text-foreground"
              >
                <option value="">{isLoading ? "Loading users" : "Select account type"}</option>
                {contextTypeOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>

            <label className="grid gap-1 text-sm font-bold" htmlFor="entity-id">
              {selectedContextType
                ? selectedContextType === "agent"
                  ? `${terminologyTitle(terminology, "agent")} or organization`
                  : terminologyTitle(terminology, selectedContextType)
                : "Account"}
              <select
                id="entity-id"
                required
                value={selectedEntityId}
                disabled={!selectedContextType || isLoading || !options}
                onChange={(event) => setSelectedEntityId(event.target.value)}
                className="h-10 w-full border border-[#0b0c0c] bg-white px-3 text-sm font-normal text-[#0b0c0c] outline-none focus:ring-2 focus:ring-[#ffdd00] disabled:border-[#b1b4b6] disabled:bg-[#f3f2f1] disabled:text-[#505a5f] dark:bg-background dark:text-foreground"
              >
                <option value="">
                  {selectedContextType ? `Select ${terminologyLabel(terminology, selectedContextType)}` : "Select account type first"}
                </option>
                {entityOptions.map((option) => (
                  <option key={option.id} value={option.id}>
                    {option.authorityName ? `${option.name} - ${option.authorityName}` : option.name}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <h2 className="mt-8 text-base font-bold">User</h2>
          <div className="mt-3 grid gap-2">
            {selectedMembership ? filteredUsers.map((identity) => {
              const context = getSelectedContextForUser(identity.id);
              return (
                <button
                  key={identity.id}
                  type="button"
                  disabled={!context}
                  onClick={() => context && submit(context)}
                  className="border border-[#b1b4b6] bg-white p-3 text-left hover:border-[#0b0c0c] disabled:pointer-events-none disabled:opacity-50 dark:bg-card"
                >
                  <span className="block text-sm font-bold">
                    {identity.displayName}
                  </span>
                  <span className="mt-1 block text-xs text-[#505a5f] dark:text-muted-foreground">
                    {identity.email}
                  </span>
                </button>
              );
            }) : (
              <div className="border border-dashed border-[#b1b4b6] p-4 text-sm text-[#505a5f] dark:text-muted-foreground">
                {isLoading ? "Loading users." : "Select an account type and account to show matching users."}
              </div>
            )}
            {selectedMembership && filteredUsers.length === 0 && (
              <div className="border border-dashed border-[#b1b4b6] p-4 text-sm text-[#505a5f] dark:text-muted-foreground">
                No active users belong to this {terminologyLabel(terminology, selectedMembership.entityType)}.
              </div>
            )}
          </div>
        </section>
      </div>
    </main>
  );
}

function buildAccountContexts(options: DemoSignInOptionsDto, terminology: undefined): RuntimeAccountContext[] {
  return options.memberships.flatMap((membership) => {
    const user = findUser(options.users, membership.userAccountId);
    if (!user) return [];
    const role = roleForContext(membership.type);

    if (membership.type === "authority") {
      const authority = findAuthority(options.authorities, membership.entityId);
      if (!authority) return [];
      return [contextFromMembership(membership, user, role, authority.id, authority.name, authority.id, authority.name, null, null)];
    }

    if (membership.type === "participant") {
      const participant = options.participants.find((item) => item.id === membership.entityId);
      const authority = findAuthority(options.authorities, participant?.authorityId);
      if (!participant || !authority) return [];
      return [contextFromMembership(membership, user, role, authority.id, authority.name, participant.id, participant.displayName, participant.id, null)];
    }

    if (membership.type === "agent") {
      const agent = options.agents.find((item) => item.id === membership.entityId);
      const authority = findAuthority(options.authorities, agent?.authorityId);
      if (!agent || !authority) return [];
      return [contextFromMembership(membership, user, role, authority.id, authority.name, agent.id, agent.displayName, null, null)];
    }

    const stakeholder = options.stakeholders.find((item) => item.id === membership.entityId);
    const authority = findAuthority(options.authorities, stakeholder?.authorityId);
    if (!stakeholder || !authority) return [];
    return [contextFromMembership(membership, user, role, authority.id, authority.name, stakeholder.id, stakeholder.displayName, null, stakeholder.id)];
  }).map((context) => ({
    ...context,
    description: context.role === "authority-admin"
      ? "Configure case templates, participants, stakeholders, and users."
      : context.role === "participant"
        ? "Complete cases, manage evidence, and control stakeholder access."
        : context.role === "agent"
          ? `Assist ${terminologyLabel(terminology, "participant", true)} where ${terminologyLabel(terminology, "agent")} access has been granted.`
          : "Review participant case that has been granted to this stakeholder account.",
  }));
}

function contextFromMembership(
  membership: DemoSignInMembershipDto,
  user: UserAccountDto,
  role: UserRole,
  authorityId: string,
  authorityName: string,
  entityId: string,
  entityName: string,
  participantId: string | null,
  stakeholderId: string | null,
): RuntimeAccountContext {
  return {
    id: `${user.id}:${membership.type}:${entityId}`,
    authenticatableUserId: user.id,
    name: user.displayName,
    email: user.email,
    entraObjectId: user.entraObjectId,
    authorityId,
    authorityName,
    role,
    entityType: membership.type,
    entityId,
    entityName,
    participantId,
    stakeholderId,
    description: "",
  };
}

function roleForContext(entityType: PrimaryContextType): UserRole {
  if (entityType === "authority") return "authority-admin";
  return entityType;
}

function findAuthority(authorities: AuthorityDto[], authorityId: string | null | undefined) {
  return authorities.find((authority) => authority.id === authorityId);
}

function findUser(users: UserAccountDto[], userAccountId: string) {
  return users.find((user) => user.id === userAccountId && user.status === "ACTIVE");
}
