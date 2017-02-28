export function SimulateSynchronousApi() {
    return Math.round(Math.random() * 100);
}

export interface IRemoteNode {
    id: number,
    type: string,
    relatedItemIds: number[]
}

export interface INode {
    id: number,
    type: string,
    children: INode[]
}

export async function GetRemoteNode(inputId: number, inputType: string): Promise<IRemoteNode> {
    await delay(100);

    var firstId = Math.round(Math.random() * 100) + 1000

    return {
        id: inputId,
        type: `${inputType}-node`,
        relatedItemIds: Range(3).map((_, i) => firstId + i)
    }
}

export function Range(max: number): number[] {
    return Array(max).fill(0).map((_, i) => i);
}

export async function delay(milliseconds: number) {
    return new Promise((resolve, reject) =>
        setTimeout(() => resolve(), milliseconds))
}

export interface IDeferred<T> {
    resolve: (x: T | Promise<T>) => void;
    reject: (x: T | Promise<T>) => void;
    promise: Promise<T>
}

export function Deferred<T>(): IDeferred<T> {
    const deferred: IDeferred<T> = {
        resolve: <any>null,
        reject: <any>null,
        promise: <any>null
    }

    deferred.promise = new Promise<T>((resolve: any, reject: any) => {
        deferred.resolve = resolve
        deferred.reject = reject
    })

    return <IDeferred<T>>deferred
}

export interface IThing {
    value: number;
    name: string;
}

export interface IBiggerThing {
    thing: IThing;
    otherValue: number;
}