import os
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.TFCPS.TFCPS_Tools_General import TFCPS_Tools_General


def start_local_test_service():
    current_file=os.path.abspath(__file__)
    current_folder=os.path.dirname(current_file)
    volumes_folder=os.path.join(current_folder,"Volumes")
    repository_folder:str=GeneralUtilities.resolve_relative_path("../../../../..",current_file)
    GeneralUtilities.ensure_directory_does_not_exist(volumes_folder)
    env_file_name = "Parameters.env"
    sc=ScriptCollectionCore()
    t: TFCPS_Tools_General = TFCPS_Tools_General(sc)
    env_values=dict({
        "image_mariadb":t.oci_image_manager.get_registry_address_for_image_with_default_tag(repository_folder,"MariaDB"),
        "image_adminer":t.oci_image_manager.get_registry_address_for_image_with_default_tag(repository_folder,"Adminer"),
    })
    t.ensure_env_file_is_generated(current_file, env_file_name,env_values)
    sc.start_local_test_service(current_file)


if __name__ == "__main__":
    start_local_test_service()
