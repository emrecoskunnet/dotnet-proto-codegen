import call from '@/config/grpcWeb'
import {
	HelloServiceClientImpl,
	GrpcWebImpl,
	HelloService,
	HelloRequest,
	HelloResponse,
	ListQuery,
	ObjectList,
	ObjectLookupList,
	GetByIdRequest,
	Entry,
	CreateRequest,
	UpdateRequest,
} from '@/api/hello/HelloV1'
import { TableResponse } from '@/hooks/web/useTable'
import { ComponentOptions } from '@/types/components'
const PATH_URL = import.meta.env.VITE_API_BASEPATH + 'XX'  // todo add api short link

const grpcWebImpl = new GrpcWebImpl(PATH_URL, {
	debug: false,
	metadata: call.authorizationMetadata()
})

export const HelloServiceClient: HelloService =
	new HelloServiceClientImpl(grpcWebImpl)
export const helloSayHelloApi = (
	params: HelloRequest
): Promise<IResponse<HelloResponse>> => {
	return new Promise<IResponse<HelloResponse>>((resolve, reject) => {
		HelloServiceClient.SayHello(params)
			.then((i) => resolve({ code: 'OK', data: i }))
			.catch(reject)
	})
}
export const helloGetListApi = (
	params: ListQuery
): Promise<IResponse<TableResponse<ObjectList_ObjectListItem>>> => {
	return new Promise<IResponse<TableResponse<ObjectList_ObjectListItem>>>(
		(resolve, reject) => {
			HelloServiceClient.GetList(params)
				.then((i) => {
					resolve({
						code: 'Ok',
						data: {
							total: i.total,
							list: i.items,
							pageNumber: params.pageNumber,
							pageSize: params.pageSize
						}
					} as IResponse<TableResponse<ObjectList_ObjectListItem>>)
				})
				.catch(reject)
		}
	)
}

export const helloGetLookupApi = (): Promise<IResponse<ObjectLookupList>> => {
	return new Promise<IResponse<ObjectLookupList>>((resolve, reject) => {
		HelloServiceClient.GetLookup({})
			.then((i) => {
				resolve({
					code: 'Ok',
					data: i
				} as IResponse<ObjectLookupList>)
			})
			.catch(reject)
	})
}

export const HelloGetLookupComponentOptionsApi = (): Promise<ComponentOptions[]> => {
	return new Promise<ComponentOptions[]>((resolve, reject) => {
		helloGetLookupApi()
			.then((i) => {
				const optionsHello: ComponentOptions[] = []
				optionsHello.length = 0
				optionsHello.push(
					...i.data.items.map((val) => ({
						value: val.helloId,
						label: val.helloName
					}))
				)
				resolve(optionsHello)
			})
			.catch(reject)
	})
}

export const helloGetByIdApi = (
	params: GetByIdRequest
): Promise<IResponse<Entry>> => {
	return new Promise<IResponse<Entry>>((resolve, reject) => {
		HelloServiceClient.GetById(params)
			.then((i) => resolve({ code: 'OK', data: i }))
			.catch(reject)
	})
}
export const helloCreateApi = (
	params: CreateRequest
): Promise<IResponse<Entry>> => {
	return new Promise<IResponse<Entry>>((resolve, reject) => {
		HelloServiceClient.Create(params)
			.then((i) => resolve({ code: 'OK', data: i }))
			.catch(reject)
	})
}
export const helloUpdateApi = (
	params: UpdateRequest
): Promise<IResponse<Entry>> => {
	return new Promise<IResponse<Entry>>((resolve, reject) => {
		HelloServiceClient.Update(params)
			.then((i) => resolve({ code: 'OK', data: i }))
			.catch(reject)
	})
}
export const helloDeleteApi = (ids: number[]): Promise<IResponse> => {
	return new Promise<IResponse>((resolve, reject) => {
		HelloServiceClient.Delete({
			helloId: ids[0]
		} as GetByIdRequest)
			.then((i) => resolve({ code: 'OK', data: i }))
			.catch(reject)
	})
}
