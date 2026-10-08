export * from './maintenanceRoutes.service';
import { MaintenanceRoutesService } from './maintenanceRoutes.service';
export * from './openIMSBackend.service';
import { OpenIMSBackendService } from './openIMSBackend.service';
export * from './user.service';
import { UserService } from './user.service';
export const APIS = [MaintenanceRoutesService, OpenIMSBackendService, UserService];
