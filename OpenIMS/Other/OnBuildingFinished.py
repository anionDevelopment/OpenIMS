from datetime  import timedelta 
from ScriptCollection.TFCPS.Docker.TFCPS_CodeUnitSpecific_Docker import TFCPS_CodeUnitSpecific_Docker_Functions,TFCPS_CodeUnitSpecific_Docker_CLI


def on_building_finished():
    tf:TFCPS_CodeUnitSpecific_Docker_Functions=TFCPS_CodeUnitSpecific_Docker_CLI.parse(__file__)
    tf.verify_image_is_working_via_network(None,{
        "InitialDatabaseType":"Transient",
        "EnforceVerbose":"true"
    },443,"/API/Other/Maintenance/AvailabilityCheck",True,"openims_net")
 
if __name__ == "__main__":
    on_building_finished()
