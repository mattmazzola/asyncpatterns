import * as util from './util';

// Wrapping non-async code
// Method 1: Deferred
const GetNumberDeferred = async (): Promise<number> => {
    const deferred = util.Deferred<number>()
    deferred.resolve(util.SimulateSynchronousApi())
    return deferred.promise
}

// Method 2: new Promise
const GetNumberPromise = async (): Promise<number> =>
    new Promise<number>((resolve, reject) => resolve(util.SimulateSynchronousApi()))

// Modifying return value of async call
// Method 1: Promise.then
const GetThingThen = async (): Promise<util.IThing> =>
    GetNumberPromise().then(x =>
        ({
            value: x,
            name: "SomeName"
        }))

// Method 2: Async/Await
const GetThingAwait = async (): Promise<util.IThing> =>
    ({
        value: await GetNumberPromise(),
        name: "SomeName"
    })

// Chaining Async Operations
// Method 1. Proimse.then
const GetBiggerThingThen = async () =>
    GetNumberPromise().then(number => 
        GetThingAwait().then(thing =>
            ({
                thing,
                otherValue: number
            })))

// Method 2: async/await
const GetBiggerThingAwait = async () =>
    ({
        otherValue: await GetNumberPromise(),
        thing: await GetThingAwait()
    })

// Aggregate many async tasks
// Method 1: Promise.all
const GetTotalValue = async () => 
    (await Promise.all(util.Range(20).map(x =>
        GetThingAwait()))).reduce((a, b) => a += b.value, 0)

// Branching Async tasks
// Method 1: Depth-First Promise.then
const GetTreeThen = async (): Promise<util.INode> =>
    util.GetRemoteNode(1, 'root')
        .then(rootRemoteNode => 
            Promise.all<util.INode>(rootRemoteNode.relatedItemIds.map(applicationId =>
                util.GetRemoteNode(applicationId, "application")
                    .then(applicationRemoteNode => 
                        Promise.all<util.INode>(applicationRemoteNode.relatedItemIds.map(serviceId =>
                            util.GetRemoteNode(serviceId, "service")
                                .then(serviceRemoteNode =>
                                    ({
                                        id: serviceRemoteNode.id,
                                        type: serviceRemoteNode.type,
                                        children: []
                                    }))
                        ))
                        .then(services =>
                            ({
                                id: applicationRemoteNode.id,
                                type: applicationRemoteNode.type,
                                children: services
                            }))
                )))
                .then(applications =>
                    ({
                        id: rootRemoteNode.id,
                        type: rootRemoteNode.type,
                        children: applications
                    }))
        )

// Method 2: Depth First (async await)
const GetTreeAwait = async (): Promise<util.INode> => {
    const rootRemoteNode = await util.GetRemoteNode(1, 'root')
    const applications = await Promise.all<util.INode>(rootRemoteNode.relatedItemIds.map(async applicationId => {
        const applicationRemoteNode = await util.GetRemoteNode(applicationId, "application")
        
        const services = await Promise.all<util.INode>(applicationRemoteNode.relatedItemIds.map(async serviceId => {
            const serviceRemoteNode = await util.GetRemoteNode(serviceId, "service")
            return {
                id: serviceRemoteNode.id,
                type: serviceRemoteNode.type,
                children: []
            }
        }))

        return {
            id: applicationRemoteNode.id,
            type: applicationRemoteNode.type,
            children: services
        }
    }))

    return {
        id: rootRemoteNode.id,
        type: rootRemoteNode.type,
        children: applications
    }
}

async function main() {
    const number1 = await GetNumberDeferred();
    const number2 = await GetNumberPromise();

    const then1 = await GetThingThen();
    const then2 = await GetThingAwait();

    const biggerThen1 = await GetBiggerThingThen();
    const biggerThen2 = await GetBiggerThingAwait();

    const totalValue = await GetTotalValue();

    const tree1 = await GetTreeThen();
    const tree2 = await GetTreeAwait();
}

main();