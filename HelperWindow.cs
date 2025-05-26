using System;
using System.Windows.Forms;

namespace rpf2fivem
{
    public partial class HelperWindow : Form
    {
        // Flag to indicate if input is complete  
        public bool InputFinishedFlag { get; private set; } = false;

        // Flags for all InputVehicle values
        public string VehicleName { get; private set; } = string.Empty;
        public string VehicleBrand { get; private set; } = string.Empty;
        public string VehiclePrice { get; private set; } = string.Empty;
        public string VehicleCategory { get; private set; } = string.Empty;
        public string VehicleType { get; private set; } = string.Empty;
        public string FileName { get; private set; } = string.Empty;

        public HelperWindow(string fileName)
        {
            this.FileName = fileName;   
            InitializeComponent();
            label2.Text = fileName;
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Add event handlers for the input fields to validate data  
            InputVehicleName.TextChanged += ValidateInputs;
            InputVehicleBrand.TextChanged += ValidateInputs;
            InputVehiclePrice.TextChanged += ValidateInputs;
            InputVehicleCategory.TextChanged += ValidateInputs;
            InputVehicleType.TextChanged += ValidateInputs;

            // Add click handler for the finish button  
            FinishButton.Click += InputFinished_Click;
        }

        private void ValidateInputs(object sender, EventArgs e)
        {
            // Basic validation - ensure all fields have values that are not "default"  
            bool isValid = !string.IsNullOrWhiteSpace(InputVehicleName.Text) && InputVehicleName.Text != "default" &&
                           !string.IsNullOrWhiteSpace(InputVehicleBrand.Text) && InputVehicleBrand.Text != "default" &&

                           !string.IsNullOrWhiteSpace(InputVehiclePrice.Text) && InputVehiclePrice.Text != "default" &&
                           !string.IsNullOrWhiteSpace(InputVehicleCategory.Text) && InputVehicleCategory.Text != "default" &&
                           !string.IsNullOrWhiteSpace(InputVehicleType.Text) && InputVehicleType.Text != "default";

            // Additional validation for the price field - ensure it's a valid integer  
            if (!string.IsNullOrWhiteSpace(InputVehiclePrice.Text) && InputVehiclePrice.Text != "default")
            {
                isValid &= int.TryParse(InputVehiclePrice.Text, out _);
            }

            // Enable or disable the finish button based on validation result  
            FinishButton.Enabled = isValid;
        }

        private void InputFinished_Click(object sender, EventArgs e)
        {
            // Set the flag to indicate input is complete  
            InputFinishedFlag = true;

            // Store the input values in the flags
            VehicleName = InputVehicleName.Text;
            VehicleBrand = InputVehicleBrand.Text;
            VehiclePrice = InputVehiclePrice.Text;
            VehicleCategory = InputVehicleCategory.Text;
            VehicleType = InputVehicleType.Text;

            // Close the form to return to the main form  
            this.Close();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}