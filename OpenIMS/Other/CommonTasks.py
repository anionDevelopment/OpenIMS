from ScriptCollection.TFCPS.Docker.TFCPS_CodeUnitSpecific_Docker import TFCPS_CodeUnitSpecific_Docker_Functions,TFCPS_CodeUnitSpecific_Docker_CLI


def common_tasks():
    tf:TFCPS_CodeUnitSpecific_Docker_Functions=TFCPS_CodeUnitSpecific_Docker_CLI.parse(__file__)
    tf.tfcps_Tools_General.get_resource_from_global_resource(tf.get_codeunit_folder(), "DevelopmentCertificate")
    tf.do_common_tasks(tf.get_version_of_project())#codeunit-version should always be the same as project-version


if __name__ == "__main__":
    common_tasks()
