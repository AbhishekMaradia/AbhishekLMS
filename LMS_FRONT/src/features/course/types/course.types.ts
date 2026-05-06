export interface CourseResponse {
    courseId: number;
    title: string;
    categoryId: number;
    courseMainImageUrl?: string;
    thumbnailUrl?: string;
    videoUrls: string[];
    description?: string;
    instructor: string;
    difficulty: string;
    durationHours: number;
    rating: number;
    price: number;
    isActive: boolean;
    tenantId?: number;
    orgName?: string;
}

export interface CourseVideoDto {
    id: number;
    courseId: number;
    title: string;
    description: string;
    videoUrl: string;
    createdAt: string;
    courseName: string;
    tenantId?: number;
}

export interface UserVideoProgressDto {
    userId: number;
    videoId: number;
    videoTitle: string;
    watchedPercentage: number;
    isCompleted: boolean;
    lastWatchedAt: string;
}
