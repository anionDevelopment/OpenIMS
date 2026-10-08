import { environment } from "../../environments/environment";
import packageInfo from '../../../package.json';

export class Settings {
    public static getAPIUrl(): string {
        return window.location.origin;
    }
    public static isVerbose(): boolean {
        return environment.development;
    }
    public static isDevelopment(): boolean {
        return environment.development;
    }
    public static isProduction(): boolean {
        return environment.production;
    }
    public static getAppName(): string {
        return "OpenIMS";
    }
    public static getAppVersion(): string {
        return packageInfo.version;
    }
}
