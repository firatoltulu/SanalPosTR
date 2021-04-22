import styled from 'styled-components';


export const Container = styled.div``;

export const CardDataContainer = styled.form`
  display: flex;
  background-color: #FFF;
  height: 100vh;
`;

export const CardData = styled.div`
  width: 620px;
  padding: 48px
`;

export const InputGroup = styled.div`
  display: flex;
  flex-direction: column;
  margin-bottom: 32px;
  padding: 10px;
`;

export const Text = styled.span`
  font-family: sans-serif;
  font-size: 16px;
  color: ${({typing}) => typing ? '#4BA580': '#848181'};
  margin-bottom: 8px;
`;

export const Input = styled.input`
  font-family: Arial, Helvetica, sans-serif;
  font-size: 14px;
  color: #848181;
  font-weight: bold;
  text-transform: uppercase;
  padding-bottom: ${({typing}) => typing ? '14px': '8px'};
  border: none;
  outline: none;
  border-bottom: 3px  solid;
  border-color: ${({typing}) => typing ? '#4BA580': '#848181'};
`;

export const InputContainerFlex = styled.div`
  display: flex;
`;

export const InputFlex = styled.div`
  margin-bottom: 32px;
  padding: 10px;
`;

export const Select = styled.select`
  width: 215px;
  height: 35px;
  background: white;
  color: gray;
  font-size: 14px;
  border: none;
  

  option {
    /*color: black;
    background: white;
    display: flex;
    white-space: pre;
    min-height: 20px;
    padding: 0px 2px 1px;*/
  }
`;

export const PaymentButton = styled.div`
  width: 224px;
  background-color: #31c486;
  padding: 14px; 
  border-radius: 4px;
  margin-left: 8px;
  cursor: pointer;
  transition: 0.3s  ease-in;

  :hover {
    background-color: #6991E6;
  }
`;

export const PaymentText = styled.p`
  font-family: Arial, Helvetica, sans-serif;
  font-size: 14px;
  color: #FFF;
  text-align: center;
  text-transform: uppercase;
  font-weight: bold;
`;

export const CreditCard = styled.div`
  display: flex;
  width: 600px;
  height: 500px;
  align-items: center;
  justify-content: center;
`;