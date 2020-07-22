using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Archi_Emulator
{
    public partial class Form1 : Form
    {
        string instructions;
        // constant
        public const string op_spliter = " ";
        public const char _spliter = ':';
        public const string r_type = "000000";
        public const string i_type = "101011";

        // Program Counter
        public string PC;  // reg value
        public int Cycle_Nu;

        // Pipeline Register
                                    // for sequntial excute
            // read only reg  
        public string r_IF_ID;
        public string r_ID_EX;
        public string r_EX_MEM;
        public string r_MEM_WB;
            // write only reg
        public string w_IF_ID;
        public string w_ID_EX;
        public string w_EX_MEM;
        public string w_MEM_WB;
        
        // Register
        public string[] register_file;
        public const int reg_count = 32;

        //Memory
        int mem_size = 1024;
        public Hashtable mem_array;
        
        //Instruction
        public Hashtable mem_intruction;


        public Form1()
        {
            InitializeComponent();
        }
        // intial
        void intialize()
        {
            intialze_reg_file_list();  // set register array value
            intialize_mem_array();   // set memory array value
            get_instruction();  // set instruction table
            PC = textBox1.Text;   // set PC Intial value
            Cycle_Nu = 0;
        }
        public void intialze_reg_file_list()
        {
            register_file = new string[reg_count];
            register_file[0] = "0";
            for (int i = 1; i < reg_count; i++)
                register_file[i] = (i + 100).ToString();
            write_reg_grid_view();
        }
        public void write_reg_grid_view()
        {
            string value;
            for (int i = 1; i < reg_count; i++)
            {
                value = register_file[i];
                object[] rowdata = { "$" + i.ToString(), value };
                dataGridView1.Rows.Add(rowdata);
            }
        }
        public void intialize_mem_array()
        {
           
            mem_array = new Hashtable();
            for (int i = 0; i < mem_size; i++)
                mem_array.Add ((100+(i*4)).ToString(),"99");
            write_mem_grid_view();
        }
        public void write_mem_grid_view()
        {
            for (int i = 0; i < mem_size; i++)
            {
                int key = (100 + (i * 4));
                object[] rowdata = { key.ToString(), (string)mem_array[key.ToString()] };
                dataGridView2.Rows.Add(rowdata);
            }
        }
        void get_instruction()
        {
            if (UserInput.Text.Length == 0)
                return;
            mem_intruction = new Hashtable();
            instructions = UserInput.Text;
            // seprete lines
            string[] lines = instructions.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] _instruction = lines[i].Split(_spliter);
                string pc = _instruction[0];
                string op_instraction = _instruction[1];
                mem_intruction.Add(pc, op_instraction);
            }
        }

        // control unit

        public string control_unit(string op_code)
        {
            string excu = null, mem = null, wb=null;
            string reg_dst, aluop1, aluop0, alusrc, bruch, mem_read, mem_write, reg_write, mem_to_reg; 
            if (op_code.Equals(r_type)) // r_type
            {
                reg_dst = "1";
                aluop1 = "1";
                aluop0 = "0";
                alusrc = "0";
                excu = reg_dst + aluop1 + aluop0 + alusrc;
                bruch = "0";
                mem_read = "0";
                mem_write = "0";
                mem = bruch + mem_read + mem_write;
                reg_write = "1";
                mem_to_reg = "0";
                wb = reg_write + mem_to_reg;
            }
            else if (op_code.Equals(i_type)) // i_type sw
            {
                reg_dst = "-";
                aluop1 = "0";
                aluop0 = "0";
                alusrc = "1";
                excu = reg_dst + aluop1 + aluop0 + alusrc;
                bruch = "0";
                mem_read = "0";
                mem_write = "1";
                mem = bruch + mem_read + mem_write;
                reg_write = "0";
                mem_to_reg = "-";
                wb = reg_write + mem_to_reg;
            }
            else
            {
                // not avalid instruction
            }
            return excu + op_spliter + mem + op_spliter + wb;
        }

        // register file hundeler

        int reg_file(int read_reg_1, int read_reg_2, int write_reg, string write_date, int flag_reg_write, ref string reg1_read, ref string reg2_read)
        {
            if (flag_reg_write == 0)
            {
                reg1_read = register_file[read_reg_1];
                reg2_read = register_file[read_reg_2];
                return 1;
            }
            else if (flag_reg_write == 1)
            {
                register_file[write_reg] = write_date;
            }
            return 0;
        }

        // memory hundeler

        int mem_file(string address, string write_data, int flag_write_mem, int flag_read_mem,ref string data)
        {
            if (flag_read_mem == 1)
            {
                data = (string)mem_array[address];
                return 1;
            }
            else if (flag_write_mem == 1)
            {
                mem_array.Remove(address);
                mem_array.Add(address, write_data);
                return 1;
            }
            else
            {
                // faild to access the memory
            }
            return 0;
        }

        // ALU hundeler 
        string ALU(string data1, string data2, string alu_op, string func_code)
        {
            int result;
            if (alu_op.Equals("00"))  // i type sw | lw
            {
                result = Int32.Parse(data1) + Int32.Parse(data2);
                return result.ToString();
            }
            else if (alu_op.Equals("10"))  // r type
            {
                if (func_code.Equals("100000")) // add
                {
                    result = Int32.Parse(data1) + Int32.Parse(data2);
                    return result.ToString();
                }
                else if (func_code.Equals("100010")) // subtract
                {
                    result = Int32.Parse(data1) - Int32.Parse(data2);
                    return result.ToString();
                }
                else if (func_code.Equals("100100")) // AND
                {
                    result = Int32.Parse(data1) & Int32.Parse(data2);
                    return result.ToString();
                }
                else if (func_code.Equals("100101"))  // OR
                {
                    result = Int32.Parse(data1) | Int32.Parse(data2);
                    return result.ToString();
                }
                else
                {
                    // not valid
                }
            }
            return null;
        }

        // multiplexer hundeler
        string MUX(string input1, string input2, int selector)
        {
            if (selector == 0)
            {
                return input1;
            }
            else
            {
                return input2;
            }
        }

        // Sign Extended hundeler
        public string Sign_Extend(string offset)
        {
            if (offset[0].Equals("0"))
            {
                return "0000000000000000" + offset;
            }
            else if (offset[0].Equals("1"))
            {
                return "1111111111111111" + offset;
            }
            return null;
        }

        // get instruction format
        string OP_format(string instruction,ref string type)
        {
            string op_code = instruction.Substring(0, 6);

            if (op_code.Equals(r_type))
            {
                string rs = instruction.Substring(6, 5);
                string rt = instruction.Substring(11, 5);
                string rd = instruction.Substring(16, 5);
                string shamt = instruction.Substring(21, 5);
                string funct = instruction.Substring(26, 6);
                type = r_type;
                return op_code + op_spliter + rs + op_spliter + rt + op_spliter + rd + op_spliter + shamt + op_spliter + funct;
            }
            else if (op_code.Equals(i_type))
            {
                string rs = instruction.Substring(6, 5);
                string rt = instruction.Substring(11, 5);
                string addres = instruction.Substring(16, 16);
                type = i_type;
                return op_code + op_spliter + rs + op_spliter + rt + op_spliter + addres;
            }
            else
            {
                type = null;
            }
            return null;
        }
        //                               ###################### Fetch #################################
        // Fetch state
        public void Fetch()
        {
            if (mem_intruction.ContainsKey(PC))
            {
                string instruction = (string)mem_intruction[PC];
                r_IF_ID = w_IF_ID;
                w_IF_ID = PC + _spliter + instruction;

                //PC += 4;
                int _pc = convert_binary_decimal(PC) + 4;
                PC = Convert.ToString(_pc, 2);
            }
            else
            {
                PC = "0000";
                // End of PC sequance
            }
            textBox1.Text = PC;
        }

        //                              ##################### Decode ################################
        // Decode Statg

        public void Decode()
        {
            string read_reg1 = null, read_reg2= null, address_extented = null, rt = null, rd=null;

            string[] if_id = r_IF_ID.Split(_spliter);  //get the register PL value


            string type=null;
            string[] op_parts = (OP_format(if_id[1], ref type)).Split(op_spliter[0]); // seprate the operation part

            string control_bits = control_unit(op_parts[0]); // activate the control unit and get the instructions bits

            int rs = convert_binary_decimal(op_parts[1]);  // rs       // convert string bits to int  integer address acces
            int _rt = convert_binary_decimal(op_parts[2]); // rt          // convert string bits to int

            int reg_write = Int32.Parse(control_bits[9].ToString()); // get reg_write bit

            if (type.Equals(r_type))
            {
                int i = reg_file(rs, _rt, 0, null, reg_write, ref read_reg1, ref read_reg2);
                rt = op_parts[2];  // get rt to pass
                rd = op_parts[3];  // get rd  to pass 
                address_extented = Sign_Extend(op_parts[3] + op_parts[4] + op_parts[5]);

            }
            else if (type.Equals(i_type)) // sw
            {
                int i = reg_file(rs, _rt, 0, null, reg_write, ref read_reg1, ref read_reg2);
                rt = op_parts[2];
                rd = op_parts[3].Substring(0,5);  // get the rd from address
                address_extented = Sign_Extend(op_parts[3]);

            }
            r_ID_EX = w_ID_EX;
            w_ID_EX = control_bits + op_spliter + read_reg1 + op_spliter + read_reg2 + op_spliter + address_extented + op_spliter + rt + op_spliter + rd;
        }

        //                 ####################### Excude ##########################
        // excude fun
        public void Excude()
        {
            string[] id_ex = r_ID_EX.Split(op_spliter[0]);

            string control_bits = id_ex[0];
            string alu_op = control_bits[1] +""+ control_bits[2]; // get access for the alu operation bits


        }
        private void button1_Click(object sender, EventArgs e)
        {
            intialize();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        int convert_binary_decimal(string binary)
        {
            return Convert.ToInt32(binary,2);
        }
    }
}
