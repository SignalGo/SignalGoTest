export class Call {
    id?: number;
    name: string;
    url: string;
    date?: Date;
    body?: any[];
    response?: any;
    methodId: number;
    serverId: number;
    isFav?: boolean;
    favDate?: Date;
    error?: any;
}
