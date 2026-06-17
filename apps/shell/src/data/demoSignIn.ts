import type {
  AgentDto,
  AuthorityDto,
  MembershipDto,
  ParticipantDto,
  StakeholderDto,
  UserAccountDto,
} from "./console";
import { apiConfig } from "@/lib/api/config";
import { publicApiRequest } from "@/lib/api/client";

export type DemoSignInMembershipType = "authority" | "participant" | "stakeholder" | "agent";

export type DemoSignInMembershipDto = MembershipDto & {
  type: DemoSignInMembershipType;
};

export type DemoSignInOptionsDto = {
  authorities: AuthorityDto[];
  participants: ParticipantDto[];
  stakeholders: StakeholderDto[];
  agents: AgentDto[];
  users: UserAccountDto[];
  memberships: DemoSignInMembershipDto[];
};

export async function loadDemoSignInOptions() {
  return publicApiRequest<DemoSignInOptionsDto>("/api/demo/sign-in-options", {
    headers: {
      "X-Demo-SignIn-Key": apiConfig.demoSignInKey,
    },
  });
}
