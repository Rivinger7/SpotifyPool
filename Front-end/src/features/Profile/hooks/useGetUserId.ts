import { useParams } from "react-router-dom"

const useGetUserId = () => {
	const { userId } = useParams<{ userId: string }>()

	return userId
}

export default useGetUserId
