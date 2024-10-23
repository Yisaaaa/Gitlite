import pytest
import utils

@pytest.fixture
def setup_and_cleanup():
    utils.setup()
    yield
    utils.clean_up()
    
@pytest.fixture()
def setup():
    utils.setup()