export interface OrganizationDto {
    id: number;
    orgName: string;
    orgCode: string;
    logoUrl?: string;
    logoThumbUrl?: string;
    primaryColor?: string;
    secondaryColor?: string;
    website?: string;
    isActive: boolean;
    createdAt: string;
}

export interface OrgRegisterRequest {
    orgName: string;
    orgCode: string;
    website?: string;
    logo?: File;
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    mobile: string;
}
