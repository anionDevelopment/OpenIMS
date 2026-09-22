from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI
from ScriptCollection.TFCPS.TFCPS_VisualRegressionTests import TFCPS_VisualRegressionTests


def update_visual_regression_baselines():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    TFCPS_VisualRegressionTests(tf).run(True)



if __name__ == "__main__":
    update_visual_regression_baselines()
