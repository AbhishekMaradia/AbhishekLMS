export interface OrganizationDto {
    id: number;
    orgName: string;
    logoUrl?: string;
    logoThumbUrl?: string;
    primaryColor?: string;
    secondaryColor?: string;
    website?: string;
    orgCode?: string;
    isActive: boolean;
    linkExpiredAt?: string;
    createdAt: string;
}

export interface OrgRegisterRequest {
    orgName: string;
    website?: string;
    logo?: File;
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    mobile: string;
}
