import { tableHeaderData } from "../../data/tableHeader"
import { TableHead, TableHeader, TableRow } from "@/components/ui/table"

const PlaylistTableHeader = () => {
	return (
		<TableHeader>
			<TableRow className="hover:bg-transparent bg-transparent">
				{tableHeaderData.map((header) => (
					<TableHead key={header.key} className={header.classname}>
						{header.title}
					</TableHead>
				))}
			</TableRow>
		</TableHeader>
	)
}

export default PlaylistTableHeader
