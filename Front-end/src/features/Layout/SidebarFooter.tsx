import {Link} from "react-router-dom";
import {Globe} from "lucide-react";

const SidebarFooter = () => {
    return (
        <div className="overflow-hidden">
            <div
                className="left-sidebar-legal-links text-[#b3b3b3] text-[11px] m-8 ml-0 mr-0 p-6 pt-0 pb-0">
                <div className="flex flex-wrap linklists-container">
                    <div className="group">
                        <Link
                            to={"https://www.spotify.com/vn-vi/legal/end-user-agreement/"}
                            target="_blank"
                            className="mr-4 leading-7"
                        >
                            <span className="group-hover:text-white">Legal</span>
                        </Link>
                    </div>
                    <div className="group">
                        <Link
                            to={"https://www.spotify.com/vn-vi/safetyandprivacy"}
                            target="_blank"
                            className="mr-4 leading-7"
                        >
                            <span className="group-hover:text-white">Safety & Privacy Center</span>
                        </Link>
                    </div>
                    <div className="group">
                        <Link
                            to={"https://www.spotify.com/vn-vi/legal/privacy-policy/"}
                            target="_blank"
                            className="mr-4 leading-7"
                        >
                            <span className="group-hover:text-white">Privacy Policy</span>
                        </Link>
                    </div>
                    <div className="group">
                        <Link
                            to={"https://www.spotify.com/vn-vi/legal/cookies-policy/"}
                            target="_blank"
                            className="mr-4 leading-7"
                        >
                            <span className="group-hover:text-white">Cookies</span>
                        </Link>
                    </div>
                    <div className="group">
                        <Link
                            to={"https://www.spotify.com/vn-vi/legal/privacy-policy/#s3"}
                            target="_blank"
                            className="mr-4 leading-7"
                        >
                            <span className="group-hover:text-white">About Ads</span>
                        </Link>
                    </div>
                    <div className="group">
                        <Link
                            to={"https://www.spotify.com/vn-vi/accessibility"}
                            target="_blank"
                            className="mr-4 leading-7"
                        >
                            <span className="group-hover:text-white">Accessibility</span>
                        </Link>
                    </div>
                </div>
            </div>
            <div className="p-6 pt-0 pb-0 mb-8 left-sidebar-language">
                <button
                    className="rounded-full border border-[#7c7c7c] p-4 pt-1 pb-1 font-bold cursor-pointer inline-flex items-center justify-center gap-1 text-[14px] hover:border-white hover:scale-105">
                    <span>
                        <Globe className="size-4"/>
                    </span>{" "}
                    English
                </button>
            </div>
        </div>
    )
}
export default SidebarFooter
