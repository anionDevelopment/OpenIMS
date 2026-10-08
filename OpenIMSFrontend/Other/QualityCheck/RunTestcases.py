from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI
from ScriptCollection.TFCPS.TFCPS_VisualRegressionTests import TFCPS_VisualRegressionTests


def run_testcases():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.run_testcases()
    vrt=TFCPS_VisualRegressionTests(tf)
    vrt.run(False)
    # The two checks answer different questions: the run above compares every screenshot with the baseline of its
    # own browser (which detects a change of the user-interface), while the two checks below compare the browsers
    # with each other (which detects that a page looks different in one browser than in the others).
    vrt.check_screenshots_are_similar()
    vrt.check_layouts_are_similar()



if __name__ == "__main__":
    run_testcases()
