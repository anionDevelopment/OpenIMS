from ScriptCollection.TFCPS.DotNet.TFCPS_CodeUnitSpecific_DotNet import TFCPS_CodeUnitSpecific_DotNet_Functions,TFCPS_CodeUnitSpecific_DotNet_CLI
from ScriptCollection.TFCPS.DotNet.CertificateGeneratorInformationGenerate import CertificateGeneratorInformationGenerate

 
def common_tasks():
    tf:TFCPS_CodeUnitSpecific_DotNet_Functions=TFCPS_CodeUnitSpecific_DotNet_CLI.parse(__file__)
    tf.tfcps_Tools_General.get_resource_from_global_resource(tf.get_codeunit_folder(), "DevelopmentCertificate")
    tf.do_common_tasks(tf.get_version_of_project(),CertificateGeneratorInformationGenerate())#codeunit-version should alsways be the same as project-version
    tf.tfcps_Tools_General.t4_transform(tf.get_codeunit_folder(),True,tf.use_cache())

if __name__ == "__main__":
    common_tasks()
